using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Application.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly ILoggedInUser _loggedInUser;
        private readonly IUnitOfWork _unitOfWork;

        public RecurringTransactionService(ILoggedInUser loggedInUser, IUnitOfWork unitOfWork)
        {
            _loggedInUser = loggedInUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<RecurringTransactions>> GetRecuringTransactionsAsync()
        {
            var user = _loggedInUser.CurrentLoggedInUser();
            var allRecuringTransaction = await _unitOfWork.RecurringTransaction.GetAllPopulatedAsync(rt => rt.UserId == user, include: q => q.Include(q => q.Transaction));
            await _unitOfWork.RecurringTransaction.GetAllPopulatedAsync(include: q => q.Include(q => q.User));
            await _unitOfWork.RecurringTransaction.GetAllPopulatedAsync(include: q => q.Include(q => q.Transaction.Category));
            return allRecuringTransaction.ToList();
        }

        public async Task DeleteRecurringTransaction(Guid recurringTransactionId)
        {
            var recurredTransaction = await _unitOfWork.RecurringTransaction.GetAsync(t => t.RecurringTransactionId == recurringTransactionId);
            var transaction = await _unitOfWork.Transaction.GetAsync(t => t.TransactionId == recurredTransaction.TransactionId);
            if (recurredTransaction != null && transaction!=null)
            {
                transaction.TransactionTimeLine = Core.Enums.TransactionTimeLine.OneTime;
                transaction.RecurrenceFrequency = null;
                recurredTransaction.IsActive = false;
                recurredTransaction.NextTransactionDate = null;
            }
            _unitOfWork.Transaction.Update(transaction);
            _unitOfWork.RecurringTransaction.Update(recurredTransaction);
            _unitOfWork.SaveAsync();
        }

    }
}
