var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

builder.Services
    .Configure<PaymentProcessorServiceConfig>(builder.Configuration.GetSection("PaymentProcessorService"))
    .AddHttpClient()
    .AddSingleton(_ => new PaymentRepository(builder.Configuration["ConnectionStrings:Postgres"]))
    .AddSingleton<PaymentProcessorFacade>()
    .AddSingleton<PaymentChannel>();

foreach (var _ in Enumerable.Range(0, 10))
{
    builder.Services.AddHostedService<PaymentChannelProcessor>();
}

var app = builder.Build();
app.AddPaymentEndpoints();

if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();