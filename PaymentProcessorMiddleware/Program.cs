var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();
var api_default = Environment.GetEnvironmentVariable("PAYMENT_PROCESSOR_URL_DEFAULT") ?? "http://payment-processor-default:8080";
var api_fallback =Environment.GetEnvironmentVariable("PAYMENT_PROCESSOR_URL_FALLBACK") ?? "http://payment-processor-fallback:8080"; 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/payments", async (Transaction transaction, IHttpClientFactory httpClientFactory) =>
{   
    var client = httpClientFactory.CreateClient();
    var requestUri = new Uri($"{api_default}/payments");
    

    var requestedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    var paymentBody = new Payment(transaction.CorrelationId, transaction.Amount, requestedAt);

    app.Logger.LogInformation($"Object being sent: CorrelationId: {paymentBody.CorrelationId}, Amount: {paymentBody.Amount}, RequestedAt: {requestedAt}");

    using var response = await client.PostAsJsonAsync(requestUri, paymentBody);

    var responseMessage = await response.Content.ReadAsStringAsync();

    app.Logger.LogInformation(responseMessage);
})
.WithName("payments")
.WithOpenApi();

app.Run();

record Transaction(Guid CorrelationId, decimal Amount);
record Payment(Guid CorrelationId, decimal Amount, string RequestedAt);
