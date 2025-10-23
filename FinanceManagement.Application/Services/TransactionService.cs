using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggedInUser _loggedInUser;
        public TransactionService(IUnitOfWork unitOfWork, ILoggedInUser loggedInUser)
        {
            _unitOfWork = unitOfWork;
            _loggedInUser = loggedInUser;
        }

        public async Task<AddTransactionVM> CreateView()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var categories = await _unitOfWork.Category.GetAllAsync(u => u.UserId == userId);
            AddTransactionVM addTransactionVM = new AddTransactionVM()
            {
                UserId = userId,
                Categories = (List<Category>)categories,
            };
            return addTransactionVM;
        }

        public async Task AddTransactionAsync(AddTransactionVM addTransactionVM)
        {
            var user = await _unitOfWork.User.GetByIdAsync(addTransactionVM.UserId);
            if (user == null)
            {
                return;
            }
            var category = await _unitOfWork.Category.GetAsync(u => u.CategoryId == addTransactionVM.CategoryId);
            if (category == null)
            {
                return;
            }

            Transaction transaction = new Transaction()
            {
                UserId = user.UserId,
                Amount = addTransactionVM.Amount,
                Description = addTransactionVM.Description,
                TransactionDate = addTransactionVM.TransactionDate,
                CategoryId = addTransactionVM.CategoryId
            };

            await _unitOfWork.Transaction.AddAsync(transaction);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<Transaction>> DisplayTransactions()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            IEnumerable<Transaction> allTransaction = await _unitOfWork.Transaction.GetAllPopulatedAsync(u => u.UserId == userId, include: q => q.Include(t => t.Category));

            return allTransaction;
        }

        public async Task<AddTransactionVM> EditView(Guid transactionId)
        {
            var transaction = await _unitOfWork.Transaction.GetAsync(t => t.TransactionId == transactionId);
            var userId = _loggedInUser.CurrentLoggedInUser();
            var categories = await _unitOfWork.Category.GetAllAsync(u => u.UserId == userId);
            AddTransactionVM addTransactionVM = new AddTransactionVM()
            {
                UserId = userId,
                Categories = (List<Category>)categories,
                Amount = transaction.Amount,
                CategoryId = transaction.CategoryId,
                Description = transaction.Description,
                CategoryType = transaction.Category.CategoryType,
                TransactionDate = transaction.TransactionDate,
            };
            return addTransactionVM;

        }

        public async Task Edit(AddTransactionVM addTransactionVM)
        {
            var user = await _unitOfWork.User.GetByIdAsync(addTransactionVM.UserId);
            if (user == null)
            {
                return;
            }
            var category = await _unitOfWork.Category.GetAsync(u => u.CategoryId == addTransactionVM.CategoryId);
            if (category == null)
            {
                return;
            }
            var transaction = await _unitOfWork.Transaction.GetAsync(t => t.TransactionId == addTransactionVM.TransactionId);
            transaction.TransactionDate = addTransactionVM.TransactionDate;
            transaction.Amount = addTransactionVM.Amount;
            transaction.Description = addTransactionVM.Description;
            transaction.CategoryId = addTransactionVM.CategoryId;
            transaction.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Transaction.Update(transaction);
            await _unitOfWork.SaveAsync();


        }

        public async Task Delete(Guid transactionId)
        {
            var transaction = await _unitOfWork.Transaction.GetAsync(t => t.TransactionId == transactionId);
            _unitOfWork.Transaction.Delete(transaction);
            await _unitOfWork.SaveAsync();
        }
    }
}
