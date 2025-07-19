namespace Infrastructure;

public class PaymentProcessorClient(IHttpClientFactory httpClientFactory)
{    
    public async Task<Result> SendPaymentToService(Payment payment, string url,CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();
        var uri = new Uri($"{url}/payments");
        var paymentBody = new
        {
            correlationId = payment.CorrelationId,
            amount = payment.Amount,
            requestedAt = payment.CreatedOn.ToString("O")
        };
            
        using var response = await client.PostAsJsonAsync(uri, paymentBody, cancellationToken);
        return response.IsSuccessStatusCode
            ? Result.Ok()
            : Result.Fail($"Failed to send payment to {url} service.");
    }
}