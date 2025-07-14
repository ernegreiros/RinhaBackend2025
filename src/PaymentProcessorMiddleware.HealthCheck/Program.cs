using PaymentProcessorMiddleware.HealthCheck;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<DefaultPaymentProcessorHealth>();
builder.Services.AddSingleton<FallbackPaymentProcessorHealth>();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/payment-processor-health", (
        [FromServices] DefaultPaymentProcessorHealth defaultPaymentProcessorHealth,
        [FromServices] FallbackPaymentProcessorHealth fallbackPaymentProcessorHealth) => Results.Ok
    (
        new
        {
            DefaultPaymentProcessor = new
            {
                defaultPaymentProcessorHealth.Failing,
                defaultPaymentProcessorHealth.MinResponseTime,
            },
            FallbackPaymentProcessor = new
            {
                fallbackPaymentProcessorHealth.Failing,
                fallbackPaymentProcessorHealth.MinResponseTime,
            }
        }
    ))
    .WithName("GetPaymentsProcessorHealth");

app.Run();
