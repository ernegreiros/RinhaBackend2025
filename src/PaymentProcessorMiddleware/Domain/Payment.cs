namespace Domain;

public record Payment(decimal Amount, string Service, DateTimeOffset CreatedOn);