using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Application.Services
{
    public class SavingService : ISavingService
    {
        private readonly ILoggedInUser _loggedInUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SavingService> _logger;
        public SavingService(ILoggedInUser loggedInUser, IUnitOfWork unitOfWork, ILogger<SavingService> logger)
        {
            _loggedInUser = loggedInUser;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<SavingVM> CreateVM()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            if (userId == Guid.Empty)
            {
                _logger.LogError("SavingService: User Id not Found");
                throw new UserNotFoundException("User Id not Found while creating view for Saving");
            }

            var userBaseCurrencySymbol = await _unitOfWork.User.GetPopulatedAsync(u => u.UserId == userId, include: q => q.Include(c => c.Currency));
            var userCategories = await _unitOfWork.Category.GetAllAsync(c => c.UserId == userId);
            if (userCategories.ToList().Count == 0)
            {
                userCategories = new List<Category>();
            }
            SavingVM savingVM = new SavingVM()
            {
                UserId = userId,
                UserCategories = userCategories.ToList(),
                CustomFrequency = false,
                UserBaseCurrency = userBaseCurrencySymbol?.Currency?.CurrencyCode,
            };
            return savingVM;
        }

        public async Task<Saving> CreateSaving(SavingVM savingVM)
        {
            if (savingVM == null)
            {
                _logger.LogError("Null SavingVm is passed to create a saving");
                throw new NullReferenceException("Saving VM is null");
            }
            Saving saving = new Saving()
            {
                UserId = savingVM.UserId,
                CategoryId = savingVM.CategoryId,
                FrequencyOfSaving = savingVM.Frequency,
                SavingAmount = savingVM.Amount,
                CustomSaving = savingVM.CustomFrequency,
                Description = savingVM.Description,
                AlreadySavedAmount = savingVM.AlreadySavedAmount,
                SavingName = savingVM.SavingName
            };

            if (savingVM.Frequency.HasValue && savingVM.CustomFrequency == false)
            {
                var savingEndDate = savingVM.Frequency;
                switch ((Frequency)savingEndDate)
                {
                    case Frequency.Daily:
                        saving.SavingEndDate = saving.SavingStartDate.AddDays(1);
                        break;
                    case Frequency.Weekly:
                        saving.SavingEndDate = saving.SavingStartDate.AddDays(7);
                        break;
                    case Frequency.Monthly:
                        saving.SavingEndDate = saving.SavingStartDate.AddMonths(1);
                        break;
                    case Frequency.Quarterly:
                        saving.SavingEndDate = saving.SavingStartDate.AddMonths(3);
                        break;
                    case Frequency.Yearly:
                        saving.SavingEndDate = saving.SavingStartDate.AddYears(1);
                        break;
                }
            }
            else
            {
                saving.SavingStartDate = (DateTime)savingVM.SavingStartDate;
                saving.SavingEndDate = (DateTime)savingVM.SavingEndDate;
            }

            if (savingVM.CategorySwitch == false)
            {
                saving.CategoryId = null;
            }
            await _unitOfWork.Saving.AddAsync(saving);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "CreateSaving: Db update Exception occured");
                throw;
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "CreateSaving: Some Database Exception Occured");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateSaving: Some unexpected exception occured");
                throw;
            }
            return saving;
        }

        public async Task<List<SavingDto>> GetUserSavings()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            if (userId == Guid.Empty)
            {
                _logger.LogError("GetUserSavings: Invalid User accessing savings");
                throw new UserNotFoundException("Invalid user id accessing savings");
            }
            var savings = await _unitOfWork.Saving.GetAllPopulatedAsync(u => u.UserId == userId, include: q => q.Include(c => c.Category));
            List<SavingDto> userSavingsList = new List<SavingDto>();

            foreach (var saving in savings)
            {
                if (saving != null)
                {
                    SavingDto savingDto = new SavingDto()
                    {
                        CategoryName = saving?.Category?.CategoryName,
                        CategoryType = saving?.Category?.CategoryType,
                        SavingAmount = saving.SavingAmount,
                        AlreadySavedAmount = saving.AlreadySavedAmount,
                        SavingEndDate = saving.SavingEndDate,
                        SavingStartDate = saving.SavingStartDate,
                        SavingName = saving.SavingName,
                        Description = saving.Description,
                        SavingId = saving.SavingId
                    };
                    userSavingsList.Add(savingDto);
                }
            }
            return userSavingsList;
        }

        public async Task<SavingVM> EditView(Guid SavingId)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var savingToEdit = await _unitOfWork.Saving.GetPopulatedAsync(b => b.SavingId == SavingId && b.UserId == userId, include: q => q.Include(u => u.User));
            if (savingToEdit == null)
            {
                _logger.LogError("EditVM: Requested saving is not found");
                throw new SavingNotFoundException("The requested saving is not found");
            }
            SavingVM savingVM = new SavingVM()
            {
                SavingId = savingToEdit.SavingId,
                UserId = savingToEdit.UserId,
                CategoryId = savingToEdit.CategoryId,
                Frequency = savingToEdit.FrequencyOfSaving,
                SavingName = savingToEdit.SavingName,
                CustomFrequency = (bool)savingToEdit.CustomSaving,
                SavingEndDate = savingToEdit.SavingEndDate,
                SavingStartDate = savingToEdit.SavingStartDate,
                Amount = savingToEdit.SavingAmount,
                AlreadySavedAmount = savingToEdit.AlreadySavedAmount,
                Description = savingToEdit.Description,
                UserBaseCurrency = savingToEdit?.User?.Currency?.CurrencyCode
            };
            if (savingVM.CategoryId != null)
            {
                savingVM.CategorySwitch = true;
            }
            else
            {
                savingVM.CategorySwitch = false;
                savingVM.CategoryId = null;
            }
            var userCategories = await _unitOfWork.Category.GetAllAsync(c => c.UserId == userId && c.CategoryType == CategoryType.Income);
            savingVM.UserCategories = userCategories.ToList();
            return savingVM;
        }

        public async Task<Saving> EditSaving(SavingVM savingVM)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var originalsaving = await _unitOfWork.Saving.GetAsync(b => b.SavingId == savingVM.SavingId && b.UserId == userId);
            if (originalsaving == null)
            {
                _logger.LogError("Original Saving not found");
                throw new SavingNotFoundException("Edit Saving: Requested Saving not found to edit");
            }
            originalsaving.SavingName = savingVM.SavingName;
            if (originalsaving.CategoryId == null && savingVM.CategorySwitch == true)
            {
                originalsaving.CategoryId = savingVM.CategoryId;
            }
            else if (originalsaving.CategoryId != null)
            {
                originalsaving.CategoryId = savingVM.CategoryId;
            }
            originalsaving.SavingAmount = savingVM.Amount;
            originalsaving.AlreadySavedAmount = savingVM.AlreadySavedAmount;
            originalsaving.Description = savingVM.Description;

            _unitOfWork.Saving.Update(originalsaving);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "EditSaving: Db Update failed");
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EditSaving: Unexpected exception occured");
                throw ex;
            }
            return originalsaving;
        }

        public async Task DeleteSaving(Guid savingId)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var savingToDelete = await _unitOfWork.Saving.GetAsync(b => b.SavingId == savingId && b.UserId == userId);
            if (savingToDelete == null)
            {
                _logger.LogError($"Delete Saving: Saving to delete not found for savingId {savingId}");
                throw new SavingNotFoundException($"Saving not found for id {savingId}");
            }
            _unitOfWork.Saving.Delete(savingToDelete);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DeleteSaving: Db Update failed");
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteSaving: Unexpected exception occured");
                throw ex;
            }
        }
    }
}
