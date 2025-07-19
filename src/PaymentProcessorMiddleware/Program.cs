using PaymentProcessorMiddleware.Infrastructure.SignalHub;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<PaymentProcessorServiceConfig>(builder.Configuration.GetSection("PaymentProcessorService"));
builder.Services.AddHttpClient();
builder.Services.AddTransient(_ => new PaymentRepository(builder.Configuration["ConnectionStrings:Postgres"]));
builder.Services.AddTransient<PaymentProcessorClient>();
builder.Services.AddSingleton(_ => new PaymentChannel());
builder.Services.AddSingleton<IRouteGlobalControlService, RouteGlobalControlService>();
builder.Services.AddHostedService<PaymentChannelProcessor>();
builder.Services.AddHostedService<SignalRWorker>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.AddPaymentEndpoints();
app.Run();