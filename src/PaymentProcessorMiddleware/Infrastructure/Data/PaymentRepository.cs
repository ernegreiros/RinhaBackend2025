namespace Infrastructure;

public class PaymentRepository
{
    private readonly string _connectionString;
    
    public PaymentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<Result> PersistPayment(Payment payment, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await using var command = connection.CreateCommand();
            command.CommandText = "insert into payment (correlationId, amount, service, createdOn) values (@correlationId, @amount, @service, @createdOn)";
            command.Parameters.AddWithValue("correlationId", payment.CorrelationId);
            command.Parameters.AddWithValue("amount", payment.Amount);
            command.Parameters.AddWithValue("service", payment.Service);
            command.Parameters.AddWithValue("createdOn", payment.CreatedOn);

            await connection.OpenAsync(cancellationToken);
            await command.ExecuteNonQueryAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception e)
        {
            var error = e.InnerException is not null
                ? e.InnerException.Message
                : e.Message; 
            
            return Result.Fail(error);
        }
    }
    
    public async Task<Result<ImmutableList<Payment>>> GetPayments(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await using var command = connection.CreateCommand();
            command.CommandText = "select correlationId, amount, service, createdOn from payment";

            if (from != DateTimeOffset.MinValue)
            {
                command.CommandText += " where createdOn >= @from ";
                command.Parameters.AddWithValue("from", from);
            }

            if (to != DateTimeOffset.MaxValue)
            {
                command.CommandText += from != DateTimeOffset.MinValue
                    ? " and createdOn <= @to" 
                    : " where createdOn <= @to";
                
                command.Parameters.AddWithValue("to", to);
            }

            await connection.OpenAsync(cancellationToken);
            
            var payments = new List<Payment>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var correlationId = reader.GetGuid(reader.GetOrdinal("correlationId"));
                var amount = reader.GetDecimal(reader.GetOrdinal("amount"));
                var service = reader.GetString(reader.GetOrdinal("service"));
                var createdOn = reader.GetDateTime(reader.GetOrdinal("createdOn"));

                payments.Add(new Payment(correlationId, amount, service, createdOn));
            }

            return Result.Ok(payments.ToImmutableList());
        }
        catch (Exception e)
        {
            var error = e.InnerException is not null
                ? e.InnerException.Message
                : e.Message;
            
            return Result.Fail(error);
        }
    }
}