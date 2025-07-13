namespace PaymentProcessorMiddleware;

public class PingHealthCheckWorker(IHttpClientFactory httpClientFactory) : BackgroundService
{
    private Timer? _timerCheck;
    private readonly Uri _uri = new($"{Consts.DefaultApiAddress}/payments/service-health");
    private readonly Uri _uriFallback = new($"{Consts.FallbackApiAddress}/payments/service-health");
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _timerCheck = new Timer(async _ => await SetHealthCheckReturnAsync(cancellationToken), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    private async Task SetHealthCheckReturnAsync(CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();

        var firstTask = SetGlobalVariables(client, _uri, true, cancellationToken);
        var secondTask = SetGlobalVariables(client, _uriFallback, false, cancellationToken);

        await Task.WhenAll(firstTask, secondTask);
    }

    private static async Task SetGlobalVariables(HttpClient client, Uri uri, bool isDefault, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetFromJsonAsync<HealthCheck>(uri, cancellationToken);

            if (response == null)
                return;

            if (isDefault)
            {
                GlobalHealthState.IsDefaultApiDown = response.Failing;
                GlobalHealthState.MinTimeToWaitForDefaultApi = response.MinResponseTime;
            }
            else
            {
                GlobalHealthState.IsFallbackApiDown = response.Failing;
                GlobalHealthState.MinTimeToWaitForFallbackApi = response.MinResponseTime;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while set health check global variables. {0}", ex.Message);
        }
    }

    public override void Dispose()
    {
        _timerCheck?.Dispose();
        base.Dispose();
    }
}
