namespace PaymentProcessorMiddleware.HealthCheck;

public class PaymentProcessorServiceConfig
{
    public string DefaultUrl { get; init; }
    public string FallbackUrl { get; init; }
}