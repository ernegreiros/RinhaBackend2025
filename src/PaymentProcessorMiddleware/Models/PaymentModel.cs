namespace Models;

public record PaymentModel(Guid CorrelationId, decimal Amount)
{
    // TODO: Think about how to handle the service name
    public static implicit operator Payment(PaymentModel model)
    {
        return new Payment(model.CorrelationId, model.Amount, Consts.DefaultApiAlias, DateTimeOffset.UtcNow);
    }
}