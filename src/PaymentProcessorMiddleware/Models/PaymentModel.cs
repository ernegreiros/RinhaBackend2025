namespace Models;

public record PaymentModel(Guid CorrelationId, decimal Amount)
{
    public static implicit operator Payment(PaymentModel model)
    {
        return new Payment(model.CorrelationId, model.Amount, Consts.DefaultApiAlias, DateTimeOffset.UtcNow);
    }
}