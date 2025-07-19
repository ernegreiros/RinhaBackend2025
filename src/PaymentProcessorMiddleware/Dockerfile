# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY ["PaymentProcessorMiddleware/PaymentProcessorMiddleware.csproj", "PaymentProcessorMiddleware/"]
WORKDIR /src/PaymentProcessorMiddleware
RUN dotnet restore
COPY PaymentProcessorMiddleware/. .
RUN dotnet publish -c Release -o /app/publish -p:UseAppHost=false

# Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentProcessorMiddleware.dll"]