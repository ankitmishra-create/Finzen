using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;

namespace FinanceManagement.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<Category> AddCategoryAsync(AddCategoryVM addCategoryVM);
        Task<IEnumerable<Category>> DisplayCategoryAsync();
        Task<AddCategoryVM> GetCategoryForUpdateAsync(Guid categoryId);
        Task UpdateCategoryAsync(AddCategoryVM addCategoryVM);
        Task DeleteCategoryAsync(Guid categoryId);
    }
}
