namespace Infrastructure.Channels;

public class PaymentChannel
{
    private readonly Channel<PaymentModel> _channel;
    public PaymentChannel()
    {
        _channel = Channel.CreateUnbounded<PaymentModel>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ChannelReader<PaymentModel> Reader => _channel.Reader;

    public ChannelWriter<PaymentModel> Writer => _channel.Writer;
}
