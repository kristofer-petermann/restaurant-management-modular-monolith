using Base.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Restaurants.Commands;
using WebApp.ViewModels.OpeningHour;

namespace WebApp.Controllers;

[Authorize]
public class OpeningHoursController : AppControllerBase
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IIdentityModuleApi _identity;
    private readonly IMediator _mediator;

    public OpeningHoursController(IRestaurantsModuleApi bll, IIdentityModuleApi identity, IMediator mediator)
    {
        _bll = bll;
        _identity = identity;
        _mediator = mediator;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (IsRoot()) return RedirectToAction("Index", "OpeningHours", new { area = "Admin" });

        var openingHours = (await _bll.GetOpeningHoursAsync()).ToList();
        if (User.Identity?.IsAuthenticated == true &&
            await _identity.IsRestaurantAdminAsync(User.GetUserId()))
        {
            var allowedRestaurantIds = await _identity.GetAdminRestaurantIdsAsync(User.GetUserId());
            openingHours = openingHours.Where(o => allowedRestaurantIds.Contains(o.RestaurantId)).ToList();
        }

        return View(openingHours);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Details", "OpeningHours", new { area = "Admin", id });
        if (id == null) return NotFound();

        var openingHour = await _bll.FindOpeningHourAsync(id.Value);
        if (openingHour == null) return NotFound();

        if (User.Identity?.IsAuthenticated == true &&
            await _identity.IsRestaurantAdminAsync(User.GetUserId()) &&
            !await _identity.IsRestaurantAdminAsync(User.GetUserId(), openingHour.RestaurantId))
        {
            return NotFound();
        }

        return View(openingHour);
    }

    public async Task<IActionResult> Create()
    {
        if (IsRoot()) return RedirectToAction("Create", "OpeningHours", new { area = "Admin" });
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId())) return Forbid();

        var vm = new OpeningHourCreateViewModel();
        await PopulateRestaurantOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OpeningHourCreateViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Create", "OpeningHours", new { area = "Admin" });
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), vm.RestaurantId)) return Forbid();

        if (ModelState.IsValid)
        {
            await _mediator.Send(new CreateOpeningHourCommand(
                vm.RestaurantId,
                vm.DayOfWeek,
                vm.OpenTime,
                vm.CloseTime,
                string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Edit", "OpeningHours", new { area = "Admin", id });
        if (id == null) return NotFound();

        var openingHour = await _bll.FindOpeningHourAsync(id.Value);
        if (openingHour == null) return NotFound();
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), openingHour.RestaurantId)) return NotFound();

        var vm = new OpeningHourEditViewModel
        {
            Id = openingHour.Id,
            RestaurantId = openingHour.RestaurantId,
            DayOfWeek = openingHour.DayOfWeek,
            OpenTime = openingHour.OpenTime,
            CloseTime = openingHour.CloseTime,
            Summary = openingHour.Summary
        };

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, OpeningHourEditViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Edit", "OpeningHours", new { area = "Admin", id });
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindOpeningHourAsync(id);
        if (entity == null) return NotFound();
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), entity.RestaurantId) ||
            !await _identity.IsRestaurantAdminAsync(User.GetUserId(), vm.RestaurantId))
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var updated = await _mediator.Send(new UpdateOpeningHourCommand(
                    entity.Id,
                    vm.RestaurantId,
                    vm.DayOfWeek,
                    vm.OpenTime,
                    vm.CloseTime,
                    string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
                if (updated == null) return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _bll.FindOpeningHourAsync(entity.Id) == null) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "OpeningHours", new { area = "Admin", id });
        if (id == null) return NotFound();

        var openingHour = await _bll.FindOpeningHourAsync(id.Value);
        if (openingHour == null) return NotFound();
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), openingHour.RestaurantId)) return NotFound();

        return View(openingHour);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "OpeningHours", new { area = "Admin", id });

        var openingHour = await _bll.FindOpeningHourAsync(id);
        if (openingHour == null) return NotFound();
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), openingHour.RestaurantId)) return NotFound();

        var removed = await _mediator.Send(new DeleteOpeningHourCommand(openingHour.Id));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<Shared.Contracts.Identity.AllowedRestaurantDto>> GetAllowedRestaurantsAsync()
    {
        return await _identity.GetAllowedRestaurantsAsync(User.GetUserId());
    }

    private async Task PopulateRestaurantOptionsAsync(OpeningHourCreateViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = (await GetAllowedRestaurantsAsync())
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();
    }

    private async Task PopulateRestaurantOptionsAsync(OpeningHourEditViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = (await GetAllowedRestaurantsAsync())
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();
    }
}
