namespace Infrastructure.Channels;

public class PaymentChannelProcessor(PaymentChannel paymentChannel) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var payment in paymentChannel.Reader.ReadAllAsync(stoppingToken))
        {
            //Process the payment
        }
    }
}
