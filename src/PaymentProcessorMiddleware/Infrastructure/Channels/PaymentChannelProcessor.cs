namespace Infrastructure;

public class PaymentChannelProcessor : BackgroundService
{
    private readonly PaymentChannel _paymentChannel;
    private readonly PaymentRepository _paymentRepository;
    private readonly PaymentProcessorClient _paymentProcessorClient;
    private readonly IRouteGlobalControlService _routeGlobalControlManager;

    public PaymentChannelProcessor(
        PaymentChannel paymentChannel,
        PaymentRepository paymentRepository,
        PaymentProcessorClient paymentProcessorFacade,
        IRouteGlobalControlService routeGlobalControlManager)
    {
        _paymentChannel = paymentChannel;
        _paymentRepository = paymentRepository;
        _paymentProcessorClient = paymentProcessorFacade;
        _routeGlobalControlManager = routeGlobalControlManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workers = Enumerable.Range(0, 50).Select(_ => Task.Run(() => ProcessPaymentsAsync(stoppingToken)));
        await Task.WhenAll(workers);
    }

    private async Task ProcessPaymentsAsync(CancellationToken stoppingToken)
    {
        await foreach (var payment in _paymentChannel.Reader.ReadAllAsync(stoppingToken))
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            var success = await TrySendWithRetryAsync(payment, stoppingToken);
            if (!success)
            {
                Console.WriteLine("Re-enqueueing payment after retries...");
                await _paymentChannel.Writer.WriteAsync(payment, stoppingToken);
            }
        }
    }

    private async Task<bool> TrySendWithRetryAsync(Payment payment, CancellationToken stoppingToken)
    {
        const int maxAttempts = 3;
        int attempt = 0;
        while (attempt < maxAttempts && !stoppingToken.IsCancellationRequested)
        {
            var control = _routeGlobalControlManager.Get();

            if (control.Failing)
            {
                await Task.Delay(10, stoppingToken); // Waiting up to the service is ready
                continue;
            }

            var response = await _paymentProcessorClient.SendPaymentToService(payment, control.Url, stoppingToken);

            if (response.IsSuccess)
            {
                await _paymentRepository.PersistPaymentAsync(payment with { Service = control.Service }, stoppingToken);
                return true;
            }

            attempt++;
            await Task.Delay(50 * attempt, stoppingToken); // Backoff
        }

        return false;
    }
}
