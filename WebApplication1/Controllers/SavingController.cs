using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Web.Controllers
{
    public class SavingController : Controller
    {
        private readonly ISavingService _savingService;
        private readonly ILogger<SavingController> _logger;

        public SavingController(ISavingService savingService, ILogger<SavingController> logger)
        {
            _savingService = savingService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var savings = await _savingService.GetUserSavings();
            return View(savings);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var savingVM = await _savingService.CreateVM();
                return View(savingVM);
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogError(ex, "User Not Found for the expected User");
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(SavingVM savingVM)
        {
            if (savingVM.CustomFrequency)
            {
                if (!savingVM.SavingStartDate.HasValue || !savingVM.SavingEndDate.HasValue)
                {
                    ModelState.AddModelError(nameof(savingVM.SavingStartDate), "Start and End Dates are required when using custom frequency.");
                }
                else if (savingVM.SavingEndDate <= savingVM.SavingStartDate)
                {
                    ModelState.AddModelError(nameof(savingVM.SavingEndDate), "End Date must be greater than Start Date.");
                }
            }
            else
            {
                savingVM.SavingStartDate = null;
                savingVM.SavingEndDate = null;
                if (savingVM.Frequency == null)
                {
                    ModelState.AddModelError(nameof(savingVM.Frequency), "Saving Frequency is required when not using custom frequency.");
                }
            }

            if (savingVM.CategorySwitch && !savingVM.CategoryId.HasValue)
            {
                ModelState.AddModelError(nameof(savingVM.CategoryId), "Select Category is required when assigning to a category.");
            }
            if (savingVM.Amount <= 0)
            {
                ModelState.AddModelError(nameof(savingVM.Amount), "Saving amount must be greater than zero.");
            }
            if (string.IsNullOrWhiteSpace(savingVM.Description))
            {
                ModelState.AddModelError(nameof(savingVM.Description), "Description is required.");
            }
            await _savingService.CreateSaving(savingVM);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                SavingVM savingVM = await _savingService.EditView(id);
                return View(savingVM);
            }
            catch (SavingNotFoundException ex)
            {
                _logger.LogError(ex, "Saving not found for savingId: {savingId}", id);
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Exception Occured");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SavingVM savingVM)
        {
            try
            {
                var editedSaving = await _savingService.EditSaving(savingVM);
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DbUpdate exception occured while updating savings (Edit Saving service)");
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occured while updating savings (Edit Saving service)");
                return View("Error");
            }
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _savingService.DeleteSaving(id);
                return RedirectToAction("Index");
            }
            catch (SavingNotFoundException ex)
            {
                _logger.LogError(ex, "Saving to delete not found");
                return View("Error");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Db update exception while deleting the saving for id{id}");
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected exception occured for id {id}");
                return View("Error");
            }
        }
    }
}
