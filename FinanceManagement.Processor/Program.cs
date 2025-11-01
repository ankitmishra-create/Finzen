using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence;
using FinanceManagement.Infrastructure.Persistence.External;
using FinanceManagement.Infrastructure.Persistence.Repositories;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using FinanceManagement.Infrastructure.Services;
using FinanceManagement.Processor.Services;
using Microsoft.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace FinanceManagement.Processor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
            {
                var configure = hostContext.Configuration;
                var connectionString = configure.GetConnectionString("DefaultConnection");

                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<ILoggedInUser, LoggedInUser>();
                services.AddHttpClient<ICurrencyConversionService, CurrencyConversionService>();
                services.AddScoped<RecurringTransactionProcessor>();

            }).Build();

            using (var scope = host.Services.CreateScope())
            {
                var process = scope.ServiceProvider.GetRequiredService<RecurringTransactionProcessor>();
                try
                {
                    await process.ProcessDueRecurringTransactionsAsync();
                }
                catch (Exception e)
                {
                    Environment.ExitCode = 1;
                }
            }
        }
    }
}
