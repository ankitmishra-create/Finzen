using FinanceManagement.Processor.Configuration;
using FinanceManagement.Processor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceManagement.Processor.BackgroundServices
{
    public class RecurringTransactionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RecurringTransactionBackgroundService> _logger;
        private readonly TimeSpan _executionInterval;

        public RecurringTransactionBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<RecurringTransactionBackgroundService> logger,
            IOptions<RecurringTransactionProcessorSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _executionInterval = TimeSpan.FromHours(settings.Value.ExecutionIntervalHours);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Recurring Transaction Background Service started at {Time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRecurringTransactionsWithScopedService(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing recurring transactions");
                }
                await Task.Delay(_executionInterval, stoppingToken);
            }

            _logger.LogInformation("Recurring Transaction Background Service stopped at {Time}", DateTime.UtcNow);
        }

        private async Task ProcessRecurringTransactionsWithScopedService(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<RecurringTransactionProcessor>();

            try
            {
                var pendingCount = await processor.GetPendingRecurringTransactionsCountAsync();

                if (pendingCount > 0)
                {
                    _logger.LogInformation("Found {Count} pending recurring transactions, starting processing", pendingCount);
                    await processor.ProcessRecurringTransactionsAsync();
                }
                else
                {
                    _logger.LogDebug("No pending recurring transactions found");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Recurring transaction processing was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during recurring transaction processing");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recurring Transaction Background Service is stopping...");
            await base.StopAsync(cancellationToken);
        }
    }
}
