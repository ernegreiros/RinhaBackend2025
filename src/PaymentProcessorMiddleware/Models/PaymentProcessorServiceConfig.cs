namespace Models;

public class PaymentProcessorServiceConfig
{
    public PaymentServiceConfig Default { get; init; }
    public PaymentServiceConfig Fallback { get; init; }
}

public class PaymentServiceConfig
{
    public string Url { get; init; }
    public decimal Fee { get; init; }
}