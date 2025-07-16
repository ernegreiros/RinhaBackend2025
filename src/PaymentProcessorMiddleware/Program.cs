var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

builder.Services
    .Configure<PaymentProcessorServiceConfig>(builder.Configuration.GetSection("PaymentProcessorService"))
    .AddHttpClient()
    .AddHostedService<PaymentChannelProcessor>()
    .AddTransient(_ => new PaymentRepository(builder.Configuration["ConnectionStrings:Postgres"]))
    .AddTransient<PaymentProcessorFacade>()
    .AddSingleton(_ => new PaymentChannel());

var app = builder.Build();

app.AddPaymentEndpoints();

if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();