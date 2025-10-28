using FinanceManagement.Infrastructure.Persistence;
using FinanceManagement.Infrastructure.Persistence.Repositories;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using FinanceManagement.Processor.BackgroundServices;
using FinanceManagement.Processor.Configuration;
using FinanceManagement.Processor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Processor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();
            }
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    var connectionString = configuration.GetConnectionString("DefaultConnection");

                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    services.Configure<RecurringTransactionProcessorSettings>(
                        configuration.GetSection(RecurringTransactionProcessorSettings.SectionName));

                    services.AddScoped<IUnitOfWork, UnitOfWork>();

                    services.AddScoped<RecurringTransactionProcessor>();

                    services.AddHostedService<RecurringTransactionBackgroundService>();

                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.AddDebug();
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
    }
}
