using Base.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;
using WebApp.ViewModels.Menu;

namespace WebApp.Controllers;

[Authorize]
public class MenusController : AppControllerBase
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IIdentityModuleApi _identity;
    private readonly IMediator _mediator;

    public MenusController(IRestaurantsModuleApi bll, IIdentityModuleApi identity, IMediator mediator)
    {
        _bll = bll;
        _identity = identity;
        _mediator = mediator;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var menus = (await _bll.GetMenusAsync()).ToList();

        if (User.Identity?.IsAuthenticated == true &&
            !IsRoot() &&
            await _identity.IsRestaurantAdminAsync(User.GetUserId()))
        {
            var allowedRestaurantIds = await _identity.GetAdminRestaurantIdsAsync(User.GetUserId());
            menus = menus.Where(m => allowedRestaurantIds.Contains(m.RestaurantId)).ToList();
        }

        return View(menus);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var menu = await _bll.FindMenuAsync(id.Value);
        if (menu == null) return NotFound();

        if (User.Identity?.IsAuthenticated == true &&
            !IsRoot() &&
            await _identity.IsRestaurantAdminAsync(User.GetUserId()) &&
            !await _identity.IsRestaurantAdminAsync(User.GetUserId(), menu.RestaurantId))
        {
            return NotFound();
        }

        return View(menu);
    }

    public async Task<IActionResult> Create()
    {
        if (!User.Identity!.IsAuthenticated) return Forbid();
        if (IsRoot()) return RedirectToAction("Create", "Menus", new { area = "Admin" });
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId())) return Forbid();

        var vm = new MenuCreateViewModel();
        await PopulateRestaurantOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuCreateViewModel vm)
    {
        if (!User.Identity!.IsAuthenticated) return Forbid();
        if (IsRoot()) return RedirectToAction("Create", "Menus", new { area = "Admin" });
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), vm.RestaurantId)) return Forbid();

        if (ModelState.IsValid)
        {
            await _mediator.Send(new CreateMenuCommand(
                vm.RestaurantId,
                vm.Name,
                vm.IsActive,
                string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        if (IsRoot()) return RedirectToAction("Edit", "Menus", new { area = "Admin", id });

        var menu = await _bll.FindMenuAsync(id.Value);
        if (menu == null) return NotFound();
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), menu.RestaurantId)) return NotFound();

        var vm = new MenuEditViewModel
        {
            Id = menu.Id,
            RestaurantId = menu.RestaurantId,
            Name = menu.Name,
            IsActive = menu.IsActive,
            Summary = menu.Summary
        };

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MenuEditViewModel vm)
    {
        if (id != vm.Id) return NotFound();
        if (IsRoot()) return RedirectToAction("Edit", "Menus", new { area = "Admin", id });

        var entity = await _bll.FindMenuAsync(id);
        if (entity == null) return NotFound();

        var userId = User.GetUserId();
        if (!await _identity.IsRestaurantAdminAsync(userId, entity.RestaurantId) ||
            !await _identity.IsRestaurantAdminAsync(userId, vm.RestaurantId))
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var updated = await _mediator.Send(new UpdateMenuCommand(
                    entity.Id,
                    vm.RestaurantId,
                    vm.Name,
                    vm.IsActive,
                    string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
                if (updated == null) return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _bll.FindMenuAsync(entity.Id) == null) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();
        if (IsRoot()) return RedirectToAction("Delete", "Menus", new { area = "Admin", id });

        var menu = await _bll.FindMenuAsync(id.Value);
        if (menu == null) return NotFound();
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), menu.RestaurantId)) return NotFound();

        return View(menu);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "Menus", new { area = "Admin", id });

        var menu = await _bll.FindMenuAsync(id);
        if (menu == null) return NotFound();
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), menu.RestaurantId)) return NotFound();

        var removed = await _mediator.Send(new DeleteMenuCommand(menu.Id));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<Shared.Contracts.Identity.AllowedRestaurantDto>> GetAllowedRestaurantsAsync()
    {
        return await _identity.GetAllowedRestaurantsAsync(User.GetUserId());
    }

    private async Task PopulateRestaurantOptionsAsync(MenuCreateViewModel vm, Guid? selectedRestaurantId = null)
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

    private async Task PopulateRestaurantOptionsAsync(MenuEditViewModel vm, Guid? selectedRestaurantId = null)
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
