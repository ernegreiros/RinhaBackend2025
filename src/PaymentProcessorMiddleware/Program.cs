var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddScoped(_ => new PaymentRepository(builder.Configuration["ConnectionStrings:Postgres"]));
builder.Services.AddSingleton(_ => new PaymentChannel());
builder.Services.AddHostedService(_ => new PaymentChannelProcessor(_.GetRequiredService<IHttpClientFactory>(), _.GetRequiredService<PaymentChannel>()));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.AddPaymentEndpoints();
app.Run();