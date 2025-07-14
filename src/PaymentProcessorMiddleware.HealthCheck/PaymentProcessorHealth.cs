namespace PaymentProcessorMiddleware.HealthCheck;

public class PaymentProcessorHealth
{
    public bool Failing { get; set; }
    public int MinResponseTime { get; set; }
}

public class DefaultPaymentProcessorHealth : PaymentProcessorHealth;
public class FallbackPaymentProcessorHealth : PaymentProcessorHealth;