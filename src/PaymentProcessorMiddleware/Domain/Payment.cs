namespace Domain;

public record Payment(Guid CorrelationId, decimal Amount, string Service, DateTimeOffset CreatedOn);