using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinanceManagement.Web.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Category> usersCategories = await _categoryService.DisplayCategoryAsync();
            return View(usersCategories);
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

        [HttpGet]
        public async Task<IActionResult> Edit(Guid categoryId)
        {
            var currentCategoryVM = await _categoryService.GetCategoryForUpdateAsync(categoryId);
            return View(currentCategoryVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AddCategoryVM addCategoryVM)
        {
            await _categoryService.UpdateCategoryAsync(addCategoryVM);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(Guid categoryId)
        {
            await _categoryService.DeleteCategoryAsync(categoryId);
            return RedirectToAction("Index");
        }

    }
}
