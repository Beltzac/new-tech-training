using Beltzac.AIPlay.App.Api.Contract;
using Beltzac.AIPlay.App.Api.Helpers;
using Beltzac.AIPlay.App.Api.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Beltzac.AIPlay.App.Api.Workers
{
    public class StatusUpdateWorker : BackgroundService
    {
        private readonly ILogger<StatusUpdateWorker> _logger;

        public StatusUpdateWorker(IServiceProvider services, ILogger<StatusUpdateWorker> logger)
        {
            Services = services;
            _logger = logger;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StatusUpdateWorker Hosted Service running.");
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            //TODO: melhorar isso aqui
            _logger.LogInformation("StatusUpdateWorker Hosted Service is working.");
            using var scope = Services.CreateScope();
            var hub = scope.ServiceProvider.GetRequiredService<LipHub>();
            var bus = scope.ServiceProvider.GetRequiredService<IRabbitMQHelper>();

            IConnection connection = null;           

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if(connection == null)
                    {
                        _logger.LogWarning("Conectando...");
                        connection = bus.CreateConnection(bus.GetConnectionFactory());
                    }

                    if (connection.IsOpen)
                    {
                        string json = bus.RetrieveSingleMessage("status", connection);

                        if (!string.IsNullOrEmpty(json))
                            await hub.OnUpdateProcessingStatus(JsonConvert.DeserializeObject<StatusUpdate>(json));
                        else
                        {
                            _logger.LogTrace("Aguardando status...");
                            await Task.Delay(2000);
                        }

                    }
                    else
                    {
                        _logger.LogWarning("Conexão não aberta, esperando...");
                        await Task.Delay(3000);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    await Task.Delay(3000);
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StatusUpdateWorker Hosted Service is stopping.");
            await Task.CompletedTask;
        }
    }
}
