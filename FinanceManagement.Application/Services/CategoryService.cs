using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

namespace FinanceManagement.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddCategoryAsync(AddCategoryVM addCategoryVM)
        {
            Guid convertedGuid;
            Guid.TryParse(addCategoryVM.UserId, out convertedGuid);
            Category category = new Category()
            {
                UserId = convertedGuid,
                CategoryName = addCategoryVM.CategoryName,
                CategoryType = addCategoryVM.CategoryType,
                SubType = addCategoryVM.SubType
            };
            User user = await _unitOfWork.User.GetByIdAsync(convertedGuid);
            category.User = user;
            await _unitOfWork.Category.AddAsync(category);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<Category>> DisplayCategoryAsync(string userId)
        {
            Guid convertedGuid;
            Guid.TryParse(userId, out convertedGuid);
            return await _unitOfWork.Category.GetAllAsync(u => u.UserId == convertedGuid);
        }

        public AddCategoryVM UpdateBuild(Guid id)
        {
            var category = _unitOfWork.Category.GetAsync(u => u.CategoryId == id);
            AddCategoryVM addCategoryVM = new AddCategoryVM()
            {
                CategoryName = category.Result.CategoryName,
                CategoryType = category.Result.CategoryType
            };
            return addCategoryVM;
        }

        public async Task Update(AddCategoryVM addCategoryVM)
        {
            var category = await _unitOfWork.Category.GetAsync(category => category.CategoryId == addCategoryVM.Id);

            category.CategoryName = addCategoryVM.CategoryName;
            category.CategoryType = addCategoryVM.CategoryType;
            category.UpdatedAt = DateTime.UtcNow;
            category.SubType = addCategoryVM.SubType;

            _unitOfWork.Category.Update(category);
            await _unitOfWork.SaveAsync();
        }

        public async Task Delete(AddCategoryVM addCategoryVM)
        {
            var categoryToDelete = await _unitOfWork.Category.GetAsync(c => c.CategoryId == addCategoryVM.Id);
            _unitOfWork.Category.Delete(categoryToDelete);
            await _unitOfWork.SaveAsync();
        }
    }
}
