using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Application.Services;

public class InvestmentService : IInvestmentService
{
    private readonly IUnitOfWork  _unitOfWork;
    private readonly ILoggedInUser _loggedInUser;
    private readonly ILogger<InvestmentService> _logger;
    
    public InvestmentService(IUnitOfWork unitOfWork,ILoggedInUser loggedInUser,ILogger<InvestmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _loggedInUser = loggedInUser;
        _logger = logger;
    }

    public async Task<InvestmentTransactionVM> PrepareView()
    {
        var userId = _loggedInUser.CurrentLoggedInUser();
        if (userId == null)
        {
            _logger.LogError("PrepareView user not found {userId}",userId);
            throw new UserNotFoundException($"AddInvestment user not found {userId}");
        }
        var allHolding = await _unitOfWork.Stocks.GetAllAsync(s => s.UserId == userId);
        var allTransactions = await _unitOfWork.StockTransaction.GetAllAsync(s => s.UserId == userId);  
        InvestmentTransactionVM investmentTransactionVm = new InvestmentTransactionVM()
        {
            StockHoldings = allHolding,
            StockTransactions = allTransactions
        };
        return investmentTransactionVm;
    }
    
    public async Task<InvestmentTransactionVM> AddInvestment(InvestmentTransactionVM investmentTransactionVm)
    {
        var userId = _loggedInUser.CurrentLoggedInUser();
        var user = await _unitOfWork.User.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogError("AddInvestment user not found {userId}",userId);
            throw new UserNotFoundException($"AddInvestment user not found {userId}");
        }
        
        //sell so we have to subtract
        if (investmentTransactionVm.TransactionType == TransactionType.Sell)
        {
            var stockHolding =await _unitOfWork.Stocks.GetAsync(s => s.StockSymbol == investmentTransactionVm.StockSymbol && s.UserId==userId);
            stockHolding.Quantity -= investmentTransactionVm.Quantity;
            if (stockHolding.Quantity == 0)
            {
                _unitOfWork.Stocks.Delete(stockHolding);
            }
            else
            {
                stockHolding.AveragePricePerUnit = (stockHolding.TotalAmount -
                                                    (investmentTransactionVm.Quantity *
                                                     investmentTransactionVm.PricePerUnit));
                stockHolding.LastUpdated=DateTime.Now;
                stockHolding.TotalAmount = stockHolding.Quantity * stockHolding.AveragePricePerUnit;    
                _unitOfWork.Stocks.Update(stockHolding);
            }
            StockTransactions StockTransaction = new StockTransactions()
            {
                StockSymbol = investmentTransactionVm.StockSymbol,
                Quantity = investmentTransactionVm.Quantity,
                UserId = userId,
                TransactionDate = DateTime.Now,
                AmountPerUnit = investmentTransactionVm.PricePerUnit,
                TransactionType = TransactionType.Sell
            };
            await _unitOfWork.StockTransaction.AddAsync(StockTransaction);
        }
        //buy
        else
        {
            var stockHolding = await _unitOfWork.Stocks.GetAsync(s =>
                s.StockSymbol == investmentTransactionVm.StockSymbol && s.UserId == userId);
            if (stockHolding != null)
            {
                var totalBefore = stockHolding.Quantity*stockHolding.AveragePricePerUnit;
                var totalNewBuy = investmentTransactionVm.Quantity*investmentTransactionVm.PricePerUnit;
                
                stockHolding.Quantity+=investmentTransactionVm.Quantity;
                stockHolding.AveragePricePerUnit=(totalBefore + totalNewBuy) / stockHolding.Quantity;
                stockHolding.TotalAmount=stockHolding.Quantity*stockHolding.AveragePricePerUnit;
                stockHolding.LastUpdated=DateTime.Now;
                _unitOfWork.Stocks.Update(stockHolding);
            }
            else
            {
                StockHoldings NewStockHolding = new StockHoldings()
                {
                    Quantity = investmentTransactionVm.Quantity,
                    UserId = userId,
                    LastUpdated = DateTime.Now,
                    AveragePricePerUnit = investmentTransactionVm.PricePerUnit,
                    TransactionDate = DateTime.Now,
                    StockSymbol = investmentTransactionVm.StockSymbol,
                    TotalAmount = investmentTransactionVm.Quantity * investmentTransactionVm.PricePerUnit
                };
                await _unitOfWork.Stocks.AddAsync(NewStockHolding);
            }
            StockTransactions StockTransaction = new StockTransactions()
            {
                StockSymbol = investmentTransactionVm.StockSymbol,
                Quantity = investmentTransactionVm.Quantity,
                UserId = userId,
                TransactionDate = DateTime.Now,
                AmountPerUnit = investmentTransactionVm.PricePerUnit,
                TransactionType = TransactionType.Buy
            };
            await _unitOfWork.StockTransaction.AddAsync(StockTransaction);
        }
        try
        {
            await _unitOfWork.SaveAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error while updating stock transaction {userId}", userId);
            throw new DbUpdateException($"Error while updating stock transaction {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating stock transaction {userId}", userId);
            throw ex;
        }
        return investmentTransactionVm;
    }
    
}