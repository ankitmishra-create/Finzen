using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public IUserRepository User { get; private set; }
        public ICategoryRepository Category { get; private set; }
        public ITransactionRepository Transaction { get; private set; }
        public ICurrencyRepository Currency { get; private set; }
        public IRecurringTransactionRepository RecurringTransaction { get; private set; }
        public ITransactionLogRepository TransactionLog { get; private set; }
        public IBudgetRepository Budget { get; private set; }
        public ISavingRepository Saving { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            User = new UserRepository(_db);
            Category = new CategoryRepository(_db);
            Transaction = new TransactionRepository(_db);
            Currency = new CurrencyRepository(_db);
            RecurringTransaction = new RecurringTransactionRepository(_db);
            TransactionLog = new TransactionLogRepository(_db);
            Budget = new BudgetRepository(_db);
            Saving = new SavingRepository(_db);
        }
        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
