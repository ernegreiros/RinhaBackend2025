using Microsoft.Extensions.Options;

namespace Endpoints;

public static class PaymentEndpoints
{
    public static void AddPaymentEndpoints(this WebApplication app)
    {
        app.MapGet("/payments-summary", async (
                [FromServices] PaymentRepository paymentRepository,
                [FromServices] IOptions<PaymentProcessorServiceConfig> paymentProcessorServiceConfig,
                [FromQuery(Name = "from")] string? f,
                [FromQuery(Name = "to")] string? t,
                CancellationToken cancellationToken) =>
            {
                var from = string.IsNullOrWhiteSpace(f)
                    ? DateTimeOffset.MinValue
                    : DateTimeOffset.Parse(f);

                var to = string.IsNullOrWhiteSpace(t)
                    ? DateTimeOffset.MaxValue
                    : DateTimeOffset.Parse(t);

                var paymentsResult = await paymentRepository.GetPayments(from, to, cancellationToken);
                if (paymentsResult.IsFailed)
                    return Results.Problem
                    (
                        statusCode: (int)HttpStatusCode.InternalServerError,
                        title: "Error while fetching payments",
                        detail: string.Join(',', paymentsResult.Errors)
                    );

                var processedPayments = paymentsResult.Value
                    .GroupBy(p => p.Service)
                    .ToDictionary
                    (
                        g => g.Key,
                        g => g.ToImmutableList()
                    );

                processedPayments.TryGetValue(Consts.DefaultApiAlias, out var defaultPayments);
                processedPayments.TryGetValue(Consts.FallbackApiAlias, out var fallbackPayments);

                return Results.Ok
                (
                    new
                    {
                        @default = BuildSummary(defaultPayments, paymentProcessorServiceConfig.Value.Default.Fee),
                        fallback = BuildSummary(fallbackPayments, paymentProcessorServiceConfig.Value.Fallback.Fee)
                    }
                );
            })
            .WithName("payments-summary")
            .WithOpenApi();

        app.MapPost("/payments", async (
                [FromServices] PaymentChannel paymentChannel,
                [FromBody] PaymentModel payment,
                CancellationToken cancellationToken) =>
            {
                await paymentChannel.Writer.WriteAsync(payment, cancellationToken);
                return Results.Accepted();
            })
            .WithName("payments")
            .WithOpenApi();
    }

    private static object BuildSummary(ImmutableList<Payment>? payments, decimal fee) => new
    {
        totalRequests = payments?.Count > 0
            ? payments.Count
            : 0,
        totalAmount = payments?.Count > 0
            ? payments.Sum(p => p.Amount)
            : 0,
        totalFee = payments?.Count > 0
            ? payments.Sum(p => p.Amount * fee)
            : 0,
        feePerTransaction = payments?.Count > 0
            ? fee
            : 0
    };
}
