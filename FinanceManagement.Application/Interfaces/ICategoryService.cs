using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;

namespace FinanceManagement.Application.Interfaces
{
    public interface ICategoryService
    {
        Task AddCategoryAsync(AddCategoryVM addCategoryVM);
        Task<IEnumerable<Category>> DisplayCategoryAsync(string userId);

        AddCategoryVM UpdateBuild(Guid id);
        Task Update(AddCategoryVM addCategoryVM);

        Task Delete(AddCategoryVM addCategoryVM);
    }
}
