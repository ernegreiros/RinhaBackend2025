namespace Infrastructure.Channels;

public class PaymentChannelProcessor(IHttpClientFactory httpClientFactory, PaymentChannel paymentChannel) : BackgroundService
{
    private readonly Uri _uri = new($"{Consts.DefaultApiAddress}/payments");
    private readonly Uri _uriFallback = new($"{Consts.FallbackApiAddress}/payments");
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await foreach (var payment in paymentChannel.Reader.ReadAllAsync(cancellationToken))
        {
            // Process the payment

            using var client = httpClientFactory.CreateClient();

            // Check global health state before processing
            if (!GlobalHealthState.IsDefaultApiDown)
                if (!GlobalHealthState.IsFallbackApiDown)
                {
                    // Calculate the fewest time to wait based on the health state
                    if(GlobalHealthState.MinTimeToWaitForDefaultApi < GlobalHealthState.MinTimeToWaitForFallbackApi) // Send to default API
                        await client.PostAsJsonAsync(_uri, payment, cancellationToken);
                    else // Send to fallback API
                        await client.PostAsJsonAsync(_uriFallback, payment, cancellationToken);
                }
                else // Send to default API
                    await client.PostAsJsonAsync(_uri, payment, cancellationToken);
            else
                if (!GlobalHealthState.IsFallbackApiDown) // Send to fallback API
                    await client.PostAsJsonAsync(_uriFallback, payment, cancellationToken);
                else
                    Console.WriteLine("Both API services are down");
        }
    }
}
