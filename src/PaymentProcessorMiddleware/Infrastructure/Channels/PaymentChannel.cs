namespace Infrastructure;

public class PaymentChannel
{
    private readonly Channel<Payment> _channel;
    public PaymentChannel()
    {
        _channel = Channel.CreateUnbounded<Payment>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false,
        });
    }

    public ChannelReader<Payment> Reader => _channel.Reader;

    public ChannelWriter<Payment> Writer => _channel.Writer;
}
