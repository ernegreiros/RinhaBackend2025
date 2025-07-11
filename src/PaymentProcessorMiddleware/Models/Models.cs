using Domain;
using PaymentProcessorMiddleware;

namespace Models;

public record PaymentModel(Guid CorrelationId, decimal Amount)
{
    // TODO: Think about how to handle the service name
    public Payment ToDomain() => new (Amount, Consts.DefaultApiAlias, DateTimeOffset.UtcNow);
}