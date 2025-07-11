namespace PaymentProcessorMiddleware;

public static class Consts
{
    public const string DefaultApi = "http://payment-processor-default:8080";
    public const string DefaultApiAlias = "default";
    public const decimal DefaultApiFee = 0.5m;
    
    public const string FallbackApi = "http://payment-processor-fallback:8080";
    public const string FallbackApiAlias = "fallback";
    public const decimal FallbackApiFee = 5m;
}