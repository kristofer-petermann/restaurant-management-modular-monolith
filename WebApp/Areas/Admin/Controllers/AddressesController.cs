using Base.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Areas.Admin.ViewModels.Address;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;

namespace WebApp.Areas.Admin.Controllers;

public class AddressesController : AdminBaseController
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IIdentityModuleApi _identity;
    private readonly IMediator _mediator;

    public AddressesController(IRestaurantsModuleApi bll, IIdentityModuleApi identity, IMediator mediator)
    {
        _bll = bll;
        _identity = identity;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index() => View(await _bll.GetAddressesAsync());

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var address = await _bll.FindAddressAsync(id.Value);
        if (address == null) return NotFound();

        return View(address);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new AddressCreateViewModel();
        await PopulateRestaurantOptionsAsync(vm);
        await PopulateUserOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AddressCreateViewModel vm)
    {
        if (ModelState.IsValid)
        {
            await _mediator.Send(new CreateAddressCommand(
                vm.Street,
                vm.City,
                vm.PostalCode,
                vm.Country,
                string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary,
                vm.UserId,
                vm.RestaurantId));
            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        await PopulateUserOptionsAsync(vm, vm.UserId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var address = await _bll.FindAddressAsync(id.Value);
        if (address == null) return NotFound();

        var vm = new AddressEditViewModel
        {
            Id = address.Id,
            Street = address.Street,
            City = address.City,
            PostalCode = address.PostalCode,
            Country = address.Country,
            Summary = address.Summary,
            UserId = address.UserId,
            RestaurantId = address.RestaurantId
        };

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        await PopulateUserOptionsAsync(vm, vm.UserId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AddressEditViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindAddressAsync(id);
        if (entity == null) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var updated = await _mediator.Send(new UpdateAddressCommand(
                    entity.Id,
                    vm.Street,
                    vm.City,
                    vm.PostalCode,
                    vm.Country,
                    string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary,
                    vm.UserId,
                    vm.RestaurantId));
                if (updated == null) return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.AddressExistsAsync(entity.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        await PopulateUserOptionsAsync(vm, vm.UserId);
        return View(vm);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var address = await _bll.FindAddressAsync(id.Value);
        if (address == null) return NotFound();

        return View(address);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var address = await _bll.FindAddressAsync(id);
        if (address == null) return NotFound();

        var removed = await _mediator.Send(new DeleteAddressCommand(address.Id));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRestaurantOptionsAsync(AddressEditViewModel vm, Guid? selectedRestaurantId = null)
    {
        var options = (await _bll.GetRestaurantsAsync())
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();

        options.Insert(0, new SelectListItem
        {
            Value = "",
            Text = "-- No restaurant --",
            Selected = !selectedRestaurantId.HasValue
        });

        vm.RestaurantOptions = options;
    }

    private async Task PopulateRestaurantOptionsAsync(AddressCreateViewModel vm, Guid? selectedRestaurantId = null)
    {
        var options = (await _bll.GetRestaurantsAsync())
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();

        options.Insert(0, new SelectListItem
        {
            Value = "",
            Text = "-- No restaurant --",
            Selected = !selectedRestaurantId.HasValue
        });

        vm.RestaurantOptions = options;
    }

    private async Task PopulateUserOptionsAsync(AddressEditViewModel vm, Guid? selectedUserId = null)
    {
        var options = (await _identity.GetUsersAsync())
            .OrderBy(u => u.Email)
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.Email ?? u.UserName ?? u.Id.ToString(),
                Selected = selectedUserId.HasValue && u.Id == selectedUserId.Value
            })
            .ToList();

        options.Insert(0, new SelectListItem
        {
            Value = "",
            Text = "-- No user --",
            Selected = !selectedUserId.HasValue
        });

        vm.UserOptions = options;
    }

    private async Task PopulateUserOptionsAsync(AddressCreateViewModel vm, Guid? selectedUserId = null)
    {
        var options = (await _identity.GetUsersAsync())
            .OrderBy(u => u.Email)
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.Email ?? u.UserName ?? u.Id.ToString(),
                Selected = selectedUserId.HasValue && u.Id == selectedUserId.Value
            })
            .ToList();

        options.Insert(0, new SelectListItem
        {
            Value = "",
            Text = "-- No user --",
            Selected = !selectedUserId.HasValue
        });

        vm.UserOptions = options;
    }
}
