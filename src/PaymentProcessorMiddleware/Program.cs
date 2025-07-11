using Endpoints;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddScoped(_ => new PaymentRepository(builder.Configuration["ConnectionStrings:Postgres"]));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.AddPaymentEndpoints();
app.Run();