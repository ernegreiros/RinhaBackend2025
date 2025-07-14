namespace Infrastructure.Channels;

public class PaymentChannel
{
    private readonly Channel<Payment> _channel;
    public PaymentChannel()
    {
        _channel = Channel.CreateUnbounded<Payment>(new UnboundedChannelOptions
        {
            SingleReader = true, // TODO: Test if it's gonna be necessary to use more readers
            SingleWriter = false
        });
    }

    public ChannelReader<Payment> Reader => _channel.Reader;

    public ChannelWriter<Payment> Writer => _channel.Writer;
}
