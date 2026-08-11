using Base.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shared.Contracts.Restaurants.Commands;
using WebApp.ViewModels.Address;

namespace WebApp.Controllers;

[Authorize]
public class AddressesController : AppControllerBase
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IMediator _mediator;

    public AddressesController(IRestaurantsModuleApi bll, IMediator mediator)
    {
        _bll = bll;
        _mediator = mediator;
    }

    // GET: Addresses
    public async Task<IActionResult> Index()
    {
        if (IsRoot())
        {
            return RedirectToAction("Index", "Addresses", new { area = "Admin" });
        }
            
        var userId = User.GetUserId();
        var adminRestaurantIds = await _bll.GetAllowedRestaurantIdsForAddressAsync(userId);

        var res = await _bll.GetAddressesAsync(userId);

        ViewData["CurrentUserId"] = userId;
        ViewData["AdminRestaurantIds"] = adminRestaurantIds;
        return View(res);
            
    }

    // GET: Addresses/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (IsRoot())
        {
            return RedirectToAction("Details", "Addresses", new { area = "Admin", id });
        }

        if (id == null)
        {
            return NotFound();
        }

        var userId = User.GetUserId();

        var res = await _bll.FindAddressAsync(id.Value, userId);

        if (res == null)
        {
            return NotFound();
        }

        return View(res);
    }

    // GET: Addresses/Create
    public async Task<IActionResult> Create()
    {
        if (IsRoot())
        {
            return RedirectToAction("Create", "Addresses", new { area = "Admin" });
        }

        var vm = new AddressCreateViewModel();
        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);

        return View(vm);
    }

    // POST: Addresses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AddressCreateViewModel vm)
    {
        if (IsRoot())
        {
            return RedirectToAction("Create", "Addresses", new { area = "Admin" });
        }
            
        var userId = User.GetUserId();

        if (ModelState.IsValid)
        {
            Guid? restaurantIdToSave = null;

            var allowedRestaurantIds = await _bll.GetAllowedRestaurantIdsForAddressAsync(userId);

            var isRestaurantAdmin = allowedRestaurantIds.Count > 0;

            if (isRestaurantAdmin)
            {
                // Admin can leave empty or choose one of their restaurants.
                if (!vm.RestaurantId.HasValue || allowedRestaurantIds.Contains(vm.RestaurantId.Value))
                {
                    restaurantIdToSave = vm.RestaurantId;
                }
                else
                {
                    ModelState.AddModelError(nameof(vm.RestaurantId), "Invalid restaurant selection.");
                }
            }
            // Regular user cannot set restaurant during create.

            if (ModelState.IsValid)
            {
                await _mediator.Send(new CreateAddressCommand(
                    vm.Street,
                    vm.City,
                    vm.PostalCode,
                    vm.Country,
                    string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary,
                    userId,
                    restaurantIdToSave,
                    User.GetUserId()));
                return RedirectToAction(nameof(Index));
            }
        }
        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    // GET: Addresses/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (IsRoot())
        {
            return RedirectToAction("Edit", "Addresses", new { area = "Admin", id });
        }

        if (id == null)
        {
            return NotFound();
        }

        var userId = User.GetUserId();

        var address = await _bll.FindAddressAsync(id.Value, userId);

        if (address == null)
        {
            return NotFound();
        }

        var vm = new AddressEditViewModel();
        {
            vm.Id = address.Id;
            vm.Street = address.Street;
            vm.City = address.City;
            vm.PostalCode = address.PostalCode;
            vm.Country = address.Country;
            vm.Summary = address.Summary;
            vm.RestaurantId = address.RestaurantId;
        }
            
        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
            

        return View(vm);
    }

    // POST: Addresses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AddressEditViewModel vm)
    {
        if (IsRoot())
        {
            return RedirectToAction("Edit", "Addresses", new { area = "Admin", id });
        }

        if (id != vm.Id)
        {
            return NotFound();
        }

        var userId = User.GetUserId();

        var entity = await _bll.FindAddressAsync(id, userId);
        if (entity == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var allowedRestaurantIds = await _bll.GetAllowedRestaurantIdsForAddressAsync(userId);
            var isRestaurantAdmin = allowedRestaurantIds.Count > 0;
            var restaurantIdToSave = entity.RestaurantId;

            if (isRestaurantAdmin)
            {
                // Admin can clear (null) or set one of their restaurants.
                if (!vm.RestaurantId.HasValue || allowedRestaurantIds.Contains(vm.RestaurantId.Value))
                {
                    restaurantIdToSave = vm.RestaurantId;
                }
                else
                {
                    ModelState.AddModelError(nameof(vm.RestaurantId), "Invalid restaurant selection.");
                }
            }
            // Regular user: do not change entity.RestaurantId at all.

            if (!ModelState.IsValid)
            {
                await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
                return View(vm);
            }

            try
            {
                var updated = await _mediator.Send(new UpdateAddressCommand(
                    entity.Id,
                    vm.Street,
                    vm.City,
                    vm.PostalCode,
                    vm.Country,
                    string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary,
                    entity.UserId,
                    restaurantIdToSave,
                    userId));
                if (updated == null) return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.AddressExistsAsync(entity.Id))
                {
                    return NotFound();
                }

                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    // GET: Addresses/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (IsRoot())
        {
            return RedirectToAction("Delete", "Addresses", new { area = "Admin", id });
        }

        if (id == null)
        {
            return NotFound();
        }

        var userId = User.GetUserId();

        var res = await _bll.FindAddressAsync(id.Value, userId);

        if (res == null)
        {
            return NotFound();
        }

        return View(res);
    }

    // POST: Addresses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (IsRoot())
        {
            return RedirectToAction("Delete", "Addresses", new { area = "Admin", id });
        }
            
        var removed = await _mediator.Send(new DeleteAddressCommand(id, User.GetUserId()));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRestaurantOptionsAsync(AddressCreateViewModel vm, Guid? selectedRestaurantId = null)
    {
        var restaurants = await _bll.GetAvailableRestaurantsForAddressAsync(IsRoot() ? null : User.GetUserId());

        var options = restaurants
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
        
    private async Task PopulateRestaurantOptionsAsync(AddressEditViewModel vm, Guid? selectedRestaurantId = null)
    {
        var restaurants = await _bll.GetAvailableRestaurantsForAddressAsync(IsRoot() ? null : User.GetUserId());

        var options = restaurants
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();
            
        // Optional/none option
        options.Insert(0, new SelectListItem
        {
            Value = "",
            Text = "-- No restaurant --",
            Selected = !selectedRestaurantId.HasValue
        });

        vm.RestaurantOptions = options;
    }
}
