using PaymentProcessorMiddleware.HealthCheck;
using Microsoft.AspNetCore.Mvc;
using SignalHub;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.Configure<PaymentProcessorServiceConfig>(builder.Configuration.GetSection("PaymentProcessorService"));
builder.Services.AddHostedService<Worker>();
builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true;
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapHub<PaymentHealthCheckHub>("/paymentHub");

app.Run();
