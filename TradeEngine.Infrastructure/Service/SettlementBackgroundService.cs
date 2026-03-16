using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradeEngine.Application.Interfaces;

namespace TradeEngine.Infrastructure.Service
{
    public class SettlementBackgroundService : BackgroundService, ISettlementBackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SettlementBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        public SettlementBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<SettlementBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Settlement background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunSettlementAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in settlement background service.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Settlement background service stopped.");
        }

        public async Task RunSettlementAsync(CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Running settlement cycle at {Time}", DateTime.UtcNow);

            using var scope = _scopeFactory.CreateScope();
            var settlementService = scope.ServiceProvider.GetRequiredService<ISettlementService>();

            await settlementService.SettlePendingTradesAsync();

            _logger.LogInformation("Settlement cycle completed at {Time}", DateTime.UtcNow);
        }
    }
}