# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY ["PaymentProcessorMiddleware.HealthCheck/PaymentProcessorMiddleware.HealthCheck.csproj", "PaymentProcessorMiddleware.HealthCheck/"]
WORKDIR /src/PaymentProcessorMiddleware.HealthCheck
RUN dotnet restore
COPY PaymentProcessorMiddleware.HealthCheck/. .
RUN dotnet publish -c Release -o /app/publish -p:UseAppHost=false

# Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentProcessorMiddleware.HealthCheck.dll"]