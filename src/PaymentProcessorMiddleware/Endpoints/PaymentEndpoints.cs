using System.Collections.Immutable;
using System.Net;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Models;
using PaymentProcessorMiddleware;

namespace Endpoints;

public static class PaymentEndpoints
{
    public static void AddPaymentEndpoints(this WebApplication app)
    {
        app.MapGet("/payments-summary", async (
                [FromServices] PaymentRepository paymentRepository,
                [FromQuery(Name="from")] string? f,
                [FromQuery(Name="to")] string? t,
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
                        @default = BuildSummary(defaultPayments, Consts.DefaultApiFee),
                        fallBack = BuildSummary(fallbackPayments, Consts.FallbackApiFee)
                    }
                );                
            })
            .WithName("payments-summary")
            .WithOpenApi();

        app.MapPost("/payments", async (
                [FromServices] IHttpClientFactory httpClientFactory,
                [FromServices] PaymentRepository paymentRepository,
                [FromBody] PaymentModel payment) =>
            {
                var paymentResult = await paymentRepository.PersistPayment(payment.ToDomain(), CancellationToken.None);
                if (paymentResult.IsFailed)
                    return Results.Problem
                    (
                        statusCode: (int)HttpStatusCode.InternalServerError,
                        title: "Error while processing payment",
                        detail: string.Join(',', paymentResult.Errors)
                    );

                using var client = httpClientFactory.CreateClient();
                var uri = new Uri($"{Consts.DefaultApiAddress}/payments");
                var paymentBody = new
                {
                    correlationId = payment.CorrelationId,
                    amount = payment.Amount,
                    requestedAt = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                };

                Console.WriteLine("Request received {0}", paymentBody);

                using var response = await client.PostAsJsonAsync(uri, paymentBody);

                return response.IsSuccessStatusCode
                    ? Results.Accepted()
                    : Results.Problem
                    (
                        statusCode: (int)response.StatusCode,
                        title: "Error while processing payment",
                        detail: response.ReasonPhrase
                    );
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
        // TODO: double check how to get the fee
        totalFee = payments?.Count > 0
            ? payments.Sum(p => p.Amount * fee)
            : 0,
        feePerTransaction = payments?.Count > 0
            ? fee
            : 0
    };
}
