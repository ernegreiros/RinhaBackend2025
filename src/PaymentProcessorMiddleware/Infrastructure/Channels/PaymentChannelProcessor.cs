namespace Infrastructure;

public class PaymentChannelProcessor : BackgroundService
{
    private readonly PaymentChannel _paymentChannel;
    private readonly PaymentRepository _paymentRepository;
    private readonly PaymentProcessorFacade _paymentProcessorFacade;
    
    public PaymentChannelProcessor(
        PaymentChannel paymentChannel,
        PaymentRepository paymentRepository,
        PaymentProcessorFacade paymentProcessorFacade)
    {
        _paymentChannel = paymentChannel;
        _paymentRepository = paymentRepository;
        _paymentProcessorFacade = paymentProcessorFacade;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var payment in _paymentChannel.Reader.ReadAllAsync(stoppingToken))
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            
            var responseDefaultService = await _paymentProcessorFacade.SendPaymentToDefaultService(payment, stoppingToken);
            if (responseDefaultService.IsSuccess)
            {
                await _paymentRepository.PersistPayment
                (
                    payment with { Service = Consts.DefaultApiAlias }, stoppingToken
                );
            }
            else
            {
                // This is temporary
                var responseFallbackService = await _paymentProcessorFacade.SendPaymentToFallbackService(payment, stoppingToken);
                if (responseFallbackService.IsSuccess)
                {
                    await _paymentRepository.PersistPayment
                    (
                        payment with { Service = Consts.FallbackApiAlias }, stoppingToken
                    );
                }
                
                // mechanism that checks which service is the best to use
            }
        }
    }
}
