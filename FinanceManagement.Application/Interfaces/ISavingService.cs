using FinanceManagement.Application.DTO;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;

namespace FinanceManagement.Application.Interfaces
{
    public interface ISavingService
    {
        Task<SavingVM> CreateVM();
        Task<Saving> CreateSaving(SavingVM savingVM);
        Task<List<SavingDto>> GetUserSavings();
        Task<SavingVM> EditView(Guid savingId);
        Task<Saving> EditSaving(SavingVM savingVM);
        Task DeleteSaving(Guid savingId);
    }
}
