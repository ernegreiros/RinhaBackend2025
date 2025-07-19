using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using SignalHub;

namespace PaymentProcessorMiddleware.HealthCheck;

public class Worker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DefaultPaymentProcessorHealth _default = new();
    private readonly FallbackPaymentProcessorHealth _fallback = new();
    private readonly PaymentProcessorServiceConfig _config;
    private readonly IHubContext<PaymentHealthCheckHub> _hub;
    private const string DEFAULT = "default";
    private const string FALLBACK = "fallback";

    public Worker(
        IHttpClientFactory httpClientFactory,
        IHubContext<PaymentHealthCheckHub> hub,
        IOptions<PaymentProcessorServiceConfig> paymentProcessorServiceConfig)
    {
        _httpClientFactory = httpClientFactory;
        _hub = hub;
        _config = paymentProcessorServiceConfig.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(5_000, cancellationToken);
            await Task.WhenAll
            (
                CheckHealthAsync(_config.DefaultUrl, _default, cancellationToken),
                CheckHealthAsync(_config.FallbackUrl, _fallback, cancellationToken)
            );

            if (!_default.Failing)
            {
                await _hub.Clients.All.SendAsync("ReceivePaymentHealthCheckUpdate", new RouteResponse(_config.DefaultUrl, DEFAULT, _default.Failing, _default.MinResponseTime), cancellationToken);

                //if (_fallback.Failing)
                //    await _hub.Clients.All.SendAsync("ReceivePaymentHealthCheckUpdate", new RouteResponse(_config.DefaultUrl, DEFAULT, _default.Failing, _default.MinResponseTime), cancellationToken);
                //else
                //{
                //    if (_default.MinResponseTime < _fallback.MinResponseTime)
                //        await _hub.Clients.All.SendAsync("ReceivePaymentHealthCheckUpdate", new RouteResponse(_config.DefaultUrl, DEFAULT, _default.Failing, _default.MinResponseTime), cancellationToken);
                //    else
                //        await _hub.Clients.All.SendAsync("ReceivePaymentHealthCheckUpdate", new RouteResponse(_config.FallbackUrl, FALLBACK, _fallback.Failing, _fallback.MinResponseTime), cancellationToken);
                //}
            }
            else
                await _hub.Clients.All.SendAsync("ReceivePaymentHealthCheckUpdate", new RouteResponse(_config.FallbackUrl, FALLBACK, _fallback.Failing, _fallback.MinResponseTime), cancellationToken);
        }
    }

    private async Task CheckHealthAsync(string url, PaymentProcessorHealth processorHealth, CancellationToken cancellationToken)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            var uri = new Uri(url += "/payments/service-health");
            var response = await client.GetFromJsonAsync<PaymentProcessorHealth>(uri, cancellationToken);

            Console.WriteLine($"response: {response.Failing} - {response.MinResponseTime} | url: {url}");

            processorHealth.Failing = response.Failing;
            processorHealth.MinResponseTime = response.MinResponseTime;
        }
        catch (Exception ex)
        {
            processorHealth.Failing = true;
            processorHealth.MinResponseTime = 1_000;
            
            Console.WriteLine("Error while fetching health state from {0}. {1}", url, ex.Message);
        }
    }

    public record RouteResponse(string Url, string Service, bool Failing, int MinResponseTime);
}
