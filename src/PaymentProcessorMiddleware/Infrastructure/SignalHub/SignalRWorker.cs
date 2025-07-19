using Microsoft.AspNetCore.SignalR.Client;

namespace PaymentProcessorMiddleware.Infrastructure.SignalHub
{
    public class SignalRWorker(IRouteGlobalControlService routeGlobalControlService) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var _connection = new HubConnectionBuilder()
                .WithUrl("http://host.docker.internal:10002/paymentHub")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<RouteGlobalControl>("ReceivePaymentHealthCheckUpdate", routeGlobalControlService.Update);

            await _connection.StartAsync(stoppingToken);

            Console.WriteLine("SignalR connection started.");

            // Keep the worker alive
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            await _connection.StopAsync(stoppingToken);
        }
    }
}
