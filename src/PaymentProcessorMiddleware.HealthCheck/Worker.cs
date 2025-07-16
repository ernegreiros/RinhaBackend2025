using Microsoft.Extensions.Options;

namespace PaymentProcessorMiddleware.HealthCheck;

public class Worker : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly DefaultPaymentProcessorHealth _defaultPaymentProcessorHealth;
    private readonly FallbackPaymentProcessorHealth _fallbackPaymentProcessorHealth;
    private readonly PaymentProcessorServiceConfig _paymentProcessorServiceConfig;
    
    public Worker(
        IHttpClientFactory httpClientFactory,
        DefaultPaymentProcessorHealth defaultPaymentProcessorHealth,
        FallbackPaymentProcessorHealth fallbackPaymentProcessorHealth,
        IOptions<PaymentProcessorServiceConfig> paymentProcessorServiceConfig)
    {
        _httpClient = httpClientFactory.CreateClient();
        _defaultPaymentProcessorHealth = defaultPaymentProcessorHealth;
        _fallbackPaymentProcessorHealth = fallbackPaymentProcessorHealth;
        _paymentProcessorServiceConfig = paymentProcessorServiceConfig.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(6_000, cancellationToken);
            await Task.WhenAll
            (
                CheckHealth(_paymentProcessorServiceConfig.DefaultUrl, _defaultPaymentProcessorHealth, cancellationToken),
                CheckHealth(_paymentProcessorServiceConfig.FallbackUrl, _fallbackPaymentProcessorHealth, cancellationToken)
            );
        }
    }

    private async Task CheckHealth(string url, PaymentProcessorHealth processorHealth, CancellationToken cancellationToken)
    {
        try
        {
            var uri = new Uri($"{url}/payments/service-health");
            var response = await _httpClient.GetFromJsonAsync<PaymentProcessorHealth>(uri, cancellationToken);
            
            processorHealth.Failing = response!.Failing;
            processorHealth.MinResponseTime = response.MinResponseTime;
        }
        catch (Exception ex)
        {
            processorHealth.Failing = true;
            processorHealth.MinResponseTime = 1_000;
            
            Console.WriteLine("Error while fetching health state from {0}. {1}", url, ex.Message);
        }
    }
    
    public override void Dispose()
    {
        _httpClient.Dispose();
        base.Dispose();
    }
}
