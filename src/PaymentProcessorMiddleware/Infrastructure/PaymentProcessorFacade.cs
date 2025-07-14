namespace Infrastructure;

public class PaymentProcessorFacade
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentProcessorFacade(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<Result> SendPaymentToDefaultService(Payment payment, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();
        var uri = new Uri($"{Consts.DefaultApiAddress}/payments");
        var paymentBody = new
        {
            correlationId = payment.CorrelationId,
            amount = payment.Amount,
            requestedAt = payment.CreatedOn.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
            
        using var response = await client.PostAsJsonAsync(uri, paymentBody, cancellationToken);
        return response.IsSuccessStatusCode 
            ? Result.Ok()
            : Result.Fail("Failed to send payment to the default service.");
    }
    
    public async Task<Result> SendPaymentToFallbackService(Payment payment, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();
        var uri = new Uri($"{Consts.FallbackApiAddress}/payments");
        var paymentBody = new
        {
            correlationId = payment.CorrelationId,
            amount = payment.Amount,
            requestedAt = payment.CreatedOn.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
            
        using var response = await client.PostAsJsonAsync(uri, paymentBody, cancellationToken);
        return response.IsSuccessStatusCode 
            ? Result.Ok()
            : Result.Fail("Failed to send payment to the fallback service.");
    }
}