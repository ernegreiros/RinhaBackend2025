namespace Infrastructure;

public class PaymentChannelProcessor : BackgroundService
{
    private readonly PaymentChannel _paymentChannel;
    private readonly PaymentRepository _paymentRepository;
    private readonly PaymentProcessorFacade _paymentProcessorFacade;
    private readonly ILogger<PaymentChannelProcessor> _logger;
    
    public PaymentChannelProcessor(
        PaymentChannel paymentChannel,
        PaymentRepository paymentRepository,
        PaymentProcessorFacade paymentProcessorFacade, 
        ILogger<PaymentChannelProcessor> logger)
    {
        _paymentChannel = paymentChannel;
        _paymentRepository = paymentRepository;
        _paymentProcessorFacade = paymentProcessorFacade;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var payment in _paymentChannel.Reader.ReadAllAsync(stoppingToken))
        {
            if (stoppingToken.IsCancellationRequested)
                break;
            
            var defaultResponse = await SendToDefaultService(payment, stoppingToken);
            if (defaultResponse.IsSuccess)
                continue;
            
            var fallbackResponse = await SendToFallbackService(payment, stoppingToken);
            if (fallbackResponse.IsSuccess)
                continue;

            _logger.LogError("Payment {PaymentCorrelationId} has been sent to the channel again", payment.CorrelationId);
            
            await _paymentChannel.Writer.WriteAsync(payment, stoppingToken);
        }
    }

    private async Task<Result> SendToDefaultService(Payment payment, CancellationToken stoppingToken = default)
    {
        var fallbackServiceResponse = await _paymentProcessorFacade.SendPaymentToDefaultService(payment, stoppingToken);
        if (fallbackServiceResponse.IsFailed) 
            return Result.Fail("Failed to process payment using default service");
        
        await _paymentRepository.PersistPayment
        (
            payment with { Service = Consts.DefaultApiAlias }, stoppingToken
        );
            
        return Result.Ok();
    }
    
    private async Task<Result> SendToFallbackService(Payment payment, CancellationToken stoppingToken = default)
    {
        var fallbackServiceResponse = await _paymentProcessorFacade.SendPaymentToFallbackService(payment, stoppingToken);
        if (fallbackServiceResponse.IsFailed) 
            return Result.Fail("Failed to process payment using fallback service");
        
        await _paymentRepository.PersistPayment
        (
            payment with { Service = Consts.FallbackApiAlias }, stoppingToken
        );
            
        return Result.Ok();
    }
}
