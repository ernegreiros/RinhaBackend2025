using Microsoft.Extensions.Options;

namespace Infrastructure;

public class PaymentProcessorFacade : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly PaymentProcessorServiceConfig _paymentProcessorServiceConfig;

    public PaymentProcessorFacade(IHttpClientFactory httpClientFactory, IOptions<PaymentProcessorServiceConfig> paymentProcessorServiceConfig)
    {
        _httpClient = httpClientFactory.CreateClient();
        _paymentProcessorServiceConfig = paymentProcessorServiceConfig.Value;
    }
    
    public async Task<Result> SendPaymentToDefaultService(Payment payment, CancellationToken cancellationToken)
    {
        var uri = new Uri($"{_paymentProcessorServiceConfig.Default.Url}/payments");
        var paymentBody = new
        {
            correlationId = payment.CorrelationId,
            amount = payment.Amount,
            requestedAt = payment.CreatedOn.ToString("O")
        };
            
        using var response = await _httpClient.PostAsJsonAsync(uri, paymentBody, cancellationToken);
        return response.IsSuccessStatusCode 
            ? Result.Ok()
            : Result.Fail("Failed to send payment to the default service.");
    }
    
    public async Task<Result> SendPaymentToFallbackService(Payment payment, CancellationToken cancellationToken)
    {
        var uri = new Uri($"{_paymentProcessorServiceConfig.Fallback.Url}/payments");
        var paymentBody = new
        {
            correlationId = payment.CorrelationId,
            amount = payment.Amount,
            requestedAt = payment.CreatedOn.ToString("O")
        };
            
        using var response = await _httpClient.PostAsJsonAsync(uri, paymentBody, cancellationToken);
        return response.IsSuccessStatusCode 
            ? Result.Ok()
            : Result.Fail("Failed to send payment to the fallback service.");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}