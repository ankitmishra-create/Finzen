using FinanceManagement.Core.Entities;

namespace FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository
{
    public interface IUnitOfWork
    {
        IUserRepository User { get; }
        ICategoryRepository Category { get; }
        ITransactionRepository Transaction { get; }
        ICurrencyRepository Currency { get; }
        IRecurringTransactionRepository RecurringTransaction { get; }
        ITransactionLogRepository TransactionLog { get; }
        IBudgetRepository Budget { get; }
        ISavingRepository Saving { get; }
        ITransactionSavingsOrBudgetsMappingRepository Mapping {get;}

        Task SaveAsync();
    }
}
