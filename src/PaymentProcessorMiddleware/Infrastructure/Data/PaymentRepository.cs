namespace Infrastructure;

public class PaymentRepository(string connectionString)
{
    public async Task<Result> PersistPaymentAsync(Payment payment, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
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
            
            Console.WriteLine(error);
            
            return Result.Fail(error);
        }
    }
    
    public async Task<Result<ImmutableList<Payment>>> GetPayments(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await using var command = connection.CreateCommand();
            var commandText = new StringBuilder("select correlationId, amount, service, createdOn from payment");

            if (from != DateTimeOffset.MinValue)
            {
                commandText.Append(" where createdOn >= @from ");
                command.Parameters.AddWithValue("from", from);
            }

            if (to != DateTimeOffset.MaxValue)
            {
                commandText.Append(from != DateTimeOffset.MinValue
                    ? " and createdOn <= @to" 
                    : " where createdOn <= @to");
                
                command.Parameters.AddWithValue("to", to);
            }

            await connection.OpenAsync(cancellationToken);
            
            var payments = new List<Payment>();
            command.CommandText = commandText.ToString();
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
            
            Console.WriteLine(error);
            
            return Result.Fail(error);
        }
    }
}