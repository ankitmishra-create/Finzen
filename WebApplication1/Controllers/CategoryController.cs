using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.Web.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService,ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                IEnumerable<Category> usersCategories = await _categoryService.DisplayCategoryAsync();
                return View(usersCategories);
            }
            catch(DatabaseException ex)
            {
                _logger.LogError(ex, "Database error on Category Index page.");
                return View("Error"); 
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error on Category Index page.");
                return View("Error");
            }
            
        }

        [HttpGet]
        public IActionResult Create()
        {
            AddCategoryVM addCategoryVM = new AddCategoryVM();
            return View(addCategoryVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddCategoryVM addCategoryVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (string.IsNullOrEmpty(userId))
                    {
                        return View(addCategoryVM);
                    }
                    addCategoryVM.UserId = userId;
                    if (addCategoryVM.CategoryType != Core.Enums.CategoryType.Investment)
                    {
                        addCategoryVM.SubType = null;
                    }
                    await _categoryService.AddCategoryAsync(addCategoryVM);
                    return RedirectToAction("Index");
                }
                return View(addCategoryVM);
            }
            catch (CategoryAlreadyExistException ex)
            {
                _logger.LogWarning(ex, "Create category failed: {ErrorMessage}", ex.Message);
                ModelState.AddModelError(nameof(AddCategoryVM.CategoryName), ex.Message);
                return View(addCategoryVM);
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Database error creating category.");
                ModelState.AddModelError(string.Empty, "A database error occurred. Please try again.");
                return View(addCategoryVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating category.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(addCategoryVM);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid categoryId)
        {
            try
            {
                var currentCategoryVM = await _categoryService.GetCategoryForUpdateAsync(categoryId);
                return View(currentCategoryVM);
            }
            catch (CategoryNotFoundException ex)
            {
                _logger.LogWarning(ex, "Edit (GET) category failed: {ErrorMessage}", ex.Message);
                return View("Error"); 
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Database error getting category {CategoryId} for edit.", categoryId);
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting category {CategoryId} for edit.", categoryId);
                return View("Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> Edit(AddCategoryVM addCategoryVM)
        {
            if (!ModelState.IsValid)
            {
                return View(addCategoryVM);
            }
            try
            {
                await _categoryService.UpdateCategoryAsync(addCategoryVM);
                return RedirectToAction("Index");
            }
            catch (CategoryNotFoundException ex)
            {
                _logger.LogWarning(ex, "Edit (POST) category failed: {ErrorMessage}", ex.Message);
                return View("NotFound");
            }
            catch (CategoryAlreadyExistException ex)
            {
                _logger.LogWarning(ex, "Edit (POST) category failed: {ErrorMessage}", ex.Message);
                ModelState.AddModelError(nameof(AddCategoryVM.CategoryName), ex.Message);
                return View(addCategoryVM);
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Database error updating category {CategoryId}", addCategoryVM.Id);
                ModelState.AddModelError(string.Empty, "A database error occurred. Please try again.");
                return View(addCategoryVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating category {CategoryId}", addCategoryVM.Id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(addCategoryVM);
            }
        }

        public async Task<IActionResult> Delete(Guid categoryId)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(categoryId);
                return RedirectToAction("Index");
            }
            catch (CategoryNotFoundException ex)
            {
                _logger.LogWarning(ex, "Delete category failed: {ErrorMessage}", ex.Message);
                return View("Error");
            }
            catch (DatabaseException ex)
            {
                _logger.LogError("Database exception occured {ErroMessage}", ex.Message);
                return View("Error");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting category {CategoryId}", categoryId);
                return View("Error");
            }
        }

    }
}
