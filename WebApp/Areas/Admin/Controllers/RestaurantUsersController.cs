using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Shared.Contracts.Identity;
using WebApp.Areas.Admin.ViewModels.RestaurantUser;

namespace WebApp.Areas.Admin.Controllers;

public class RestaurantUsersController : AdminBaseController
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

    public async Task<IActionResult> Index() => View(await _mediator.Send(new GetRestaurantUsersQuery()));

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var restaurantUser = await _mediator.Send(new GetRestaurantUserByIdQuery(id.Value));
        if (restaurantUser == null) return NotFound();

        return View(restaurantUser);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new RestaurantUserCreateViewModel();
        await PopulateCreateOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RestaurantUserCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCreateOptionsAsync(vm, vm.RestaurantId, vm.UserId);
            return View(vm);
        }

        await _mediator.Send(new AssignRestaurantUserCommand(
            vm.RestaurantId,
            vm.UserId,
            vm.UserRole,
            string.IsNullOrWhiteSpace(vm.Role) ? null : vm.Role,
            string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var restaurantUser = await _mediator.Send(new GetRestaurantUserByIdQuery(id.Value));
        if (restaurantUser == null) return NotFound();

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
        if (id != vm.Id) return NotFound();

        var existing = await _mediator.Send(new GetRestaurantUserByIdQuery(id));
        if (existing == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateEditOptionsAsync(vm, vm.RestaurantId, vm.UserId);
            return View(vm);
        }

        try
        {
            var updated = await _mediator.Send(new UpdateRestaurantUserCommand(
                id,
                vm.RestaurantId,
                vm.UserId,
                vm.UserRole,
                string.IsNullOrWhiteSpace(vm.Role) ? null : vm.Role,
                string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
            if (updated == null) return NotFound();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (await _mediator.Send(new GetRestaurantUserByIdQuery(id)) == null) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var restaurantUser = await _mediator.Send(new GetRestaurantUserByIdQuery(id.Value));
        if (restaurantUser == null) return NotFound();

        return View(restaurantUser);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var entity = await _mediator.Send(new GetRestaurantUserByIdQuery(id));
        if (entity == null) return NotFound();

        await _mediator.Send(new RemoveRestaurantUserCommand(entity.Id));
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCreateOptionsAsync(RestaurantUserCreateViewModel vm, Guid? selectedRestaurantId = null, Guid? selectedUserId = null)
    {
        vm.RestaurantOptions = await BuildRestaurantOptionsAsync(selectedRestaurantId);
        vm.UserOptions = await BuildUserOptionsAsync(selectedUserId);
    }

    private async Task PopulateEditOptionsAsync(RestaurantUserEditViewModel vm, Guid? selectedRestaurantId = null, Guid? selectedUserId = null)
    {
        vm.RestaurantOptions = await BuildRestaurantOptionsAsync(selectedRestaurantId);
        vm.UserOptions = await BuildUserOptionsAsync(selectedUserId);
    }

    private async Task<List<SelectListItem>> BuildRestaurantOptionsAsync(Guid? selectedRestaurantId = null)
    {
        return (await _restaurants.GetRestaurantsAsync())
            .OrderBy(r => r.RestaurantName)
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();
    }

    private async Task<List<SelectListItem>> BuildUserOptionsAsync(Guid? selectedUserId = null)
    {
        return (await _bll.GetUsersAsync())
            .OrderBy(u => u.Email)
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.Email ?? u.UserName ?? u.Id.ToString(),
                Selected = selectedUserId.HasValue && u.Id == selectedUserId.Value
            })
            .ToList();
    }
}
