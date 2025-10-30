using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggedInUser _loggedInUser;
        private readonly ILogger<CategoryService> _logger;
        public CategoryService(IUnitOfWork unitOfWork, ILoggedInUser loggedInUser, ILogger<CategoryService> logger)
        {
            _loggedInUser = loggedInUser;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Category> AddCategoryAsync(AddCategoryVM addCategoryVM)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var categoryNameLower = addCategoryVM.CategoryName.ToLower();
            var existing = await _unitOfWork.Category.GetAsync(c => c.UserId == userId && c.CategoryName.ToLower() == categoryNameLower && c.CategoryType == addCategoryVM.CategoryType);

            if (existing != null)
            {
                _logger.LogWarning("Category '{CategoryName}' already exists for user {UserId}", addCategoryVM.CategoryName, userId);
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
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database save error. Inner: {InnerMessage}", ex.InnerException?.Message);
                throw new DatabaseException($"A database error occurred while saving. {ex.InnerException?.Message}", ex);
            }
            return category;
        }

        public async Task<IEnumerable<Category>> DisplayCategoryAsync()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            try
            {
                return await _unitOfWork.Category.GetAllAsync(u => u.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error while displaying categories for user {UserId}", userId);
                throw new DatabaseException("Could not retrieve categories.", ex);
            }
        }

        public async Task<AddCategoryVM> GetCategoryForUpdateAsync(Guid categoryId)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var category = await _unitOfWork.Category.GetAsync(u => u.CategoryId == categoryId);

            if (category == null)
            {
                _logger.LogWarning("Category '{CategoryId}' does not exists for user {UserId}", categoryId, userId);
                throw new CategoryNotFoundException("Category not found!");
            }

            AddCategoryVM addCategoryVM = new AddCategoryVM()
            {
                Id = category.CategoryId,
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
                _logger.LogWarning("Category '{CategoryName}' does not exists for user {UserId}", addCategoryVM.CategoryName, userId);
                throw new CategoryNotFoundException("Category not found");
            }

            category.CategoryName = addCategoryVM.CategoryName;
            category.CategoryType = addCategoryVM.CategoryType;
            category.UpdatedAt = DateTime.UtcNow;
            category.SubType = addCategoryVM.SubType;

            _unitOfWork.Category.Update(category);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database save error. Inner: {InnerMessage}", ex.InnerException?.Message);
                throw new DatabaseException($"A database error occurred while saving. {ex.InnerException?.Message}", ex);
            }
        }

        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var categoryToDelete = await _unitOfWork.Category.GetAsync(c => c.CategoryId == categoryId && c.UserId == userId);
            if (categoryToDelete == null)
            {
                _logger.LogWarning("Category '{CategoryId}' does not exists for user {UserId}", categoryId, userId);
                throw new CategoryNotFoundException("Category not found");
            }
            _unitOfWork.Category.Delete(categoryToDelete);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database save error. Inner: {InnerMessage}", ex.InnerException?.Message);
                throw new DatabaseException($"A database error occurred while saving. {ex.InnerException?.Message}", ex);
            }
        }
    }
}
