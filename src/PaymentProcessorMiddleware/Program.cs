var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<PaymentProcessorServiceConfig>(builder.Configuration.GetSection("PaymentProcessorService"));
builder.Services.AddHttpClient();
builder.Services.AddTransient(_ => new PaymentRepository(builder.Configuration["ConnectionStrings:Postgres"]));
builder.Services.AddTransient<PaymentProcessorFacade>();
builder.Services.AddSingleton(_ => new PaymentChannel());
builder.Services.AddHostedService<PaymentChannelProcessor>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.AddPaymentEndpoints();
app.Run();