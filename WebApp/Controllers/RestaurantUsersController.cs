using Base.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Identity;
using WebApp.ViewModels.RestaurantUser;

namespace WebApp.Controllers;

[Authorize]
public class RestaurantUsersController : AppControllerBase
{
    private readonly IIdentityModuleApi _bll;
    private readonly IRestaurantsModuleApi _restaurants;
    private readonly IMediator _mediator;

    public RestaurantUsersController(IIdentityModuleApi bll, IRestaurantsModuleApi restaurants, IMediator mediator)
    {
        _bll = bll;
        _restaurants = restaurants;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        if (IsRoot()) return RedirectToAction("Index", "RestaurantUsers", new { area = "Admin" });

        var userId = User.GetUserId();
        var links = (await _mediator.Send(new GetRestaurantUsersQuery()))
            .Where(r => r.UserId == userId)
            .ToList();

        return View(links);
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Details", "RestaurantUsers", new { area = "Admin", id });
        if (id == null) return NotFound();

        var restaurantUser = await _mediator.Send(new GetRestaurantUserByIdQuery(id.Value));
        if (restaurantUser == null) return NotFound();
        if (!await CanAccessLinkAsync(restaurantUser)) return NotFound();

        return View(restaurantUser);
    }

    public async Task<IActionResult> Create()
    {
        if (IsRoot()) return RedirectToAction("Create", "RestaurantUsers", new { area = "Admin" });

        var vm = new RestaurantUserCreateViewModel();
        await PopulateCreateOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RestaurantUserCreateViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Create", "RestaurantUsers", new { area = "Admin" });

        var userId = User.GetUserId();
        var userRole = vm.UserRole;
        var role = string.IsNullOrWhiteSpace(vm.Role) ? null : vm.Role;
        var summary = string.IsNullOrWhiteSpace(vm.Summary) ? null : vm.Summary;

        if (!await IsRestaurantAdminAsync(vm.RestaurantId))
        {
            if (vm.UserId != userId) return Forbid();
            userRole = Shared.Contracts.Identity.RestaurantUserRole.Normal;
            role = "user";
        }
        else
        {
            userRole = Shared.Contracts.Identity.RestaurantUserRole.Normal;
        }

        if (ModelState.IsValid)
        {
            await _mediator.Send(new AssignRestaurantUserCommand(
                vm.RestaurantId,
                vm.UserId,
                userRole,
                role,
                summary,
                vm.UserId));
            return RedirectToAction(nameof(Index));
        }

        await PopulateCreateOptionsAsync(vm, vm.RestaurantId, vm.UserId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Edit", "RestaurantUsers", new { area = "Admin", id });
        if (id == null) return NotFound();

        var restaurantUser = await _mediator.Send(new GetRestaurantUserByIdQuery(id.Value));
        if (restaurantUser == null) return NotFound();
        if (!await CanEditLinkAsync(restaurantUser)) return NotFound();

        var vm = new RestaurantUserEditViewModel
        {
            Id = restaurantUser.Id,
            RestaurantId = restaurantUser.RestaurantId,
            UserId = restaurantUser.UserId,
            UserRole = restaurantUser.UserRole,
            Role = restaurantUser.Role,
            Summary = restaurantUser.Summary
        };

        await PopulateEditOptionsAsync(vm, vm.RestaurantId, vm.UserId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, RestaurantUserEditViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Edit", "RestaurantUsers", new { area = "Admin", id });
        if (id != vm.Id) return NotFound();

        var readEntity = await _mediator.Send(new GetRestaurantUserByIdQuery(id));
        if (readEntity == null) return NotFound();
        if (!await CanEditLinkAsync(readEntity)) return NotFound();

        vm.UserRole = readEntity.UserRole;
        if (readEntity.UserId == User.GetUserId() || !await IsRestaurantAdminAsync(vm.RestaurantId))
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var updated = await _mediator.Send(new UpdateRestaurantUserCommand(
                    id,
                    vm.RestaurantId,
                    vm.UserId,
                    vm.UserRole,
                    string.IsNullOrWhiteSpace(vm.Role) ? null : vm.Role,
                    string.IsNullOrWhiteSpace(vm.Summary) ? null : vm.Summary,
                    vm.UserId));
                if (updated == null) return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _mediator.Send(new GetRestaurantUserByIdQuery(id)) == null) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateEditOptionsAsync(vm, vm.RestaurantId, vm.UserId);
        return View(vm);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "RestaurantUsers", new { area = "Admin", id });
        if (id == null) return NotFound();

        var restaurantUser = await _mediator.Send(new GetRestaurantUserByIdQuery(id.Value));
        if (restaurantUser == null) return NotFound();
        if (!await CanDeleteLinkAsync(restaurantUser)) return NotFound();

        return View(restaurantUser);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "RestaurantUsers", new { area = "Admin", id });

        var restaurantUser = await _mediator.Send(new GetRestaurantUserByIdQuery(id));
        if (restaurantUser == null) return NotFound();
        if (!await CanDeleteLinkAsync(restaurantUser)) return NotFound();

        await _mediator.Send(new RemoveRestaurantUserCommand(restaurantUser.Id));
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> IsRestaurantAdminAsync(Guid restaurantId)
    {
        return await _bll.IsRestaurantAdminAsync(User.GetUserId(), restaurantId);
    }

    private async Task<bool> CanAccessLinkAsync(Shared.Contracts.Identity.RestaurantUserReadDto link)
    {
        return link.UserId == User.GetUserId() || await IsRestaurantAdminAsync(link.RestaurantId);
    }

    private async Task<bool> CanEditLinkAsync(Shared.Contracts.Identity.RestaurantUserReadDto link)
    {
        return await IsRestaurantAdminAsync(link.RestaurantId);
    }

    private async Task<bool> CanDeleteLinkAsync(Shared.Contracts.Identity.RestaurantUserReadDto link)
    {
        return link.UserId == User.GetUserId() || await IsRestaurantAdminAsync(link.RestaurantId);
    }

    private async Task PopulateCreateOptionsAsync(RestaurantUserCreateViewModel vm, Guid? restaurantId = null, Guid? targetUserId = null)
    {
        var currentUserId = User.GetUserId();
        var adminRestaurantIds = await _bll.GetAdminRestaurantIdsAsync(currentUserId);

        var restaurants = adminRestaurantIds.Count == 0
            ? (await _restaurants.GetRestaurantsAsync()).ToList()
            : (await _restaurants.GetRestaurantsAsync()).Where(r => adminRestaurantIds.Contains(r.Id)).ToList();

        vm.RestaurantOptions = restaurants
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = restaurantId.HasValue && r.Id == restaurantId.Value
            })
            .ToList();

        var selectedUserId = targetUserId ?? currentUserId;
        vm.UserOptions = (await _bll.GetUsersAsync())
            .Where(u => u.Id == currentUserId)
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.Email ?? u.UserName ?? u.Id.ToString(),
                Selected = u.Id == selectedUserId
            })
            .ToList();
    }

    private async Task PopulateEditOptionsAsync(RestaurantUserEditViewModel vm, Guid? restaurantId = null, Guid? targetUserId = null)
    {
        var currentUserId = User.GetUserId();
        var adminRestaurantIds = await _bll.GetAdminRestaurantIdsAsync(currentUserId);
        var restaurants = (await _restaurants.GetRestaurantsAsync())
            .Where(r => adminRestaurantIds.Contains(r.Id))
            .ToList();

        vm.RestaurantOptions = restaurants
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = restaurantId.HasValue && r.Id == restaurantId.Value
            })
            .ToList();

        vm.UserOptions = (await _bll.GetUsersAsync())
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.Email ?? u.UserName ?? u.Id.ToString(),
                Selected = targetUserId.HasValue && u.Id == targetUserId.Value
            })
            .ToList();
    }
}
