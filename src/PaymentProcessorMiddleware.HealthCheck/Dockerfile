# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY "PaymentProcessorMiddleware.HealthCheck/PaymentProcessorMiddleware.HealthCheck.csproj" .
RUN dotnet restore "PaymentProcessorMiddleware.HealthCheck.csproj"
COPY . .
WORKDIR /src/PaymentProcessorMiddleware.HealthCheck
RUN dotnet publish "PaymentProcessorMiddleware.HealthCheck.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentProcessorMiddleware.HealthCheck.dll"]