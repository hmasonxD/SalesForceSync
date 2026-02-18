using SalesForceSync.Data;

namespace SalesForceSync.Services
{
    public class SyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SyncBackgroundService> _logger;
        private readonly TimeSpan _syncInterval;

        public SyncBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<SyncBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            var minutes = configuration.GetValue<int>("SyncSettings:IntervalMinutes", 30);
            _syncInterval = TimeSpan.FromMinutes(minutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sync Background Service started. Running every {Minutes} minutes.", _syncInterval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Automatic sync triggered at {Time}", DateTime.UtcNow);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var contactService = scope.ServiceProvider.GetRequiredService<SalesforceContactService>();
                        var count = await contactService.SyncContactsAsync();
                        _logger.LogInformation("Automatic sync completed: {Count} contacts synced", count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Automatic sync failed");
                }

                await Task.Delay(_syncInterval, stoppingToken);
            }
        }
    }
}