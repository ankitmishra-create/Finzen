using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

namespace FinanceManagement.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggedInUser _loggedInUser;
        public CategoryService(IUnitOfWork unitOfWork,ILoggedInUser loggedInUser)
        {
            _loggedInUser = loggedInUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Category> AddCategoryAsync(AddCategoryVM addCategoryVM)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var existing = await _unitOfWork.Category.GetAsync(c => c.UserId == userId && c.CategoryName.ToLower() == addCategoryVM.CategoryName.ToLower() && c.CategoryType==addCategoryVM.CategoryType);
            
            if(existing != null)
            {
                throw new CategoryAlreadyExistException("A category with this name and type already exists.");
            }

            Category category = new Category()
            {
                UserId = userId,
                CategoryName = addCategoryVM.CategoryName,
                CategoryType = addCategoryVM.CategoryType,
                SubType = addCategoryVM.SubType
            };
            await _unitOfWork.Category.AddAsync(category);
            await _unitOfWork.SaveAsync();
            return category;
        }

        public async Task<IEnumerable<Category>> DisplayCategoryAsync()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            return await _unitOfWork.Category.GetAllAsync(u => u.UserId == userId);
        }

        public async Task<AddCategoryVM> GetCategoryForUpdateAsync(Guid categoryId)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var category = await _unitOfWork.Category.GetAsync(u => u.CategoryId == categoryId);

            if (category == null)
            {
                throw new CategoryNotFoundException("Category not found!");
            }

            AddCategoryVM addCategoryVM = new AddCategoryVM()
            {
                Id=category.CategoryId,
                CategoryName = category.CategoryName,
                CategoryType = category.CategoryType
            };
            return addCategoryVM;
        }

        public async Task UpdateCategoryAsync(AddCategoryVM addCategoryVM)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var category = await _unitOfWork.Category.GetAsync(category => category.CategoryId == addCategoryVM.Id);

            if (category == null)
            {
                throw new CategoryNotFoundException("Category not found");
            }

            category.CategoryName = addCategoryVM.CategoryName;
            category.CategoryType = addCategoryVM.CategoryType;
            category.UpdatedAt = DateTime.UtcNow;
            category.SubType = addCategoryVM.SubType;

            _unitOfWork.Category.Update(category);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var categoryToDelete = await _unitOfWork.Category.GetAsync(c => c.CategoryId == categoryId && c.UserId==userId);
            if (categoryToDelete == null)
            {
                throw new CategoryNotFoundException("Category not found");
            }
            _unitOfWork.Category.Delete(categoryToDelete);
            await _unitOfWork.SaveAsync();
        }
    }
}
