namespace PaymentProcessorMiddleware.HealthCheck;

public class Worker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DefaultPaymentProcessorHealth _defaultPaymentProcessorHealth;
    private readonly FallbackPaymentProcessorHealth _fallbackPaymentProcessorHealth;
    private readonly Uri _uriDefault = new("http://payment-processor-default:8080/payments/service-health");
    private readonly Uri _uriFallback = new("http://payment-processor-fallback:8080/payments/service-health");
    
    public Worker(
        IHttpClientFactory httpClientFactory,
        DefaultPaymentProcessorHealth defaultPaymentProcessorHealth,
        FallbackPaymentProcessorHealth fallbackPaymentProcessorHealth)
    {
        _httpClientFactory = httpClientFactory;
        _defaultPaymentProcessorHealth = defaultPaymentProcessorHealth;
        _fallbackPaymentProcessorHealth = fallbackPaymentProcessorHealth;
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            
            await Task.Delay(6_000, cancellationToken);
            await Task.WhenAll
            (
                CheckHealth(_uriDefault, _defaultPaymentProcessorHealth, cancellationToken),
                CheckHealth(_uriFallback, _fallbackPaymentProcessorHealth,  cancellationToken)
            );
        }
    }

    private async Task CheckHealth(Uri uri, PaymentProcessorHealth processorHealth, CancellationToken cancellationToken)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<PaymentProcessorHealth>(uri, cancellationToken);
            if (response is null)
                return;

            processorHealth.Failing = response.Failing;
            processorHealth.MinResponseTime = response.MinResponseTime;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while fetching health state from {0}. {1}", uri.Host, ex.Message);
        }
    }
}
