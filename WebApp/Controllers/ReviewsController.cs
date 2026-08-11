using Base.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;
using WebApp.ViewModels.Review;

namespace WebApp.Controllers;

[Authorize]
public class ReviewsController : AppControllerBase
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IIdentityModuleApi _identity;
    private readonly IMediator _mediator;

    public ReviewsController(IRestaurantsModuleApi bll, IIdentityModuleApi identity, IMediator mediator)
    {
        _bll = bll;
        _identity = identity;
        _mediator = mediator;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (IsRoot()) return RedirectToAction("Index", "Reviews", new { area = "Admin" });

        var reviews = (await _bll.GetReviewsAsync()).ToList();

        if (User.Identity?.IsAuthenticated == true && !IsRoot())
        {
            var userId = User.GetUserId();
            if (await _identity.IsRestaurantAdminAsync(userId))
            {
                var adminRestaurantIds = await _identity.GetAdminRestaurantIdsAsync(userId);
                reviews = reviews.Where(r => adminRestaurantIds.Contains(r.RestaurantId)).ToList();
            }
        }

        await PopulateMissingReviewEmailsAsync(reviews);
        return View(reviews);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Details", "Reviews", new { area = "Admin", id });
        if (id == null) return NotFound();

        var review = await _bll.FindReviewAsync(id.Value);
        if (review == null) return NotFound();

        if (User.Identity?.IsAuthenticated == true &&
            !IsRoot() &&
            await _identity.IsRestaurantAdminAsync(User.GetUserId()) &&
            !await _identity.IsRestaurantAdminAsync(User.GetUserId(), review.RestaurantId))
        {
            return NotFound();
        }

        await PopulateMissingReviewEmailsAsync(new[] { review });
        return View(review);
    }

    public async Task<IActionResult> Create()
    {
        if (IsRoot()) return RedirectToAction("Create", "Reviews", new { area = "Admin" });

        var vm = new ReviewCreateViewModel();
        await PopulateRestaurantOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewCreateViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Create", "Reviews", new { area = "Admin" });

        if (!await IsRestaurantSelectionAllowedAsync(vm.RestaurantId))
        {
            ModelState.AddModelError(nameof(vm.RestaurantId), "You cannot select this restaurant.");
        }

        if (ModelState.IsValid)
        {
            await _bll.PublishReviewAsync(new PublishReviewCommand(
                vm.RestaurantId,
                vm.Rating,
                vm.Comment,
                vm.Summary), User.GetUserId());
            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Edit", "Reviews", new { area = "Admin", id });
        if (id == null) return NotFound();

        var review = await _bll.FindReviewAsync(id.Value);
        if (review == null) return NotFound();

        if (review.UserId != User.GetUserId()) return NotFound();

        var vm = new ReviewEditViewModel
        {
            Id = review.Id,
            RestaurantId = review.RestaurantId,
            Rating = review.Rating,
            Comment = review.Comment,
            Summary = review.Summary
        };

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ReviewEditViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Edit", "Reviews", new { area = "Admin", id });
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindReviewAsync(id);
        if (entity == null) return NotFound();
        if (entity.UserId != User.GetUserId()) return NotFound();

        if (!await IsRestaurantSelectionAllowedAsync(vm.RestaurantId))
        {
            ModelState.AddModelError(nameof(vm.RestaurantId), "You cannot select this restaurant.");
        }

        if (ModelState.IsValid)
        {
            entity.RestaurantId = vm.RestaurantId;
            entity.Rating = vm.Rating;
            entity.Comment = string.IsNullOrWhiteSpace(vm.Comment) ? string.Empty : vm.Comment;
            entity.Summary = string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary;

            try
            {
                var updated = await _mediator.Send(new UpdateReviewCommand(
                    entity.Id,
                    entity.RestaurantId,
                    entity.UserId,
                    entity.Rating,
                    entity.Comment,
                    entity.CreatedAt,
                    entity.Summary,
                    User.GetUserId()));
                if (updated == null) return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _bll.FindReviewAsync(entity.Id) == null) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "Reviews", new { area = "Admin", id });
        if (id == null) return NotFound();

        var review = await _bll.FindReviewAsync(id.Value);
        if (review == null) return NotFound();
        if (review.UserId != User.GetUserId()) return NotFound();

        await PopulateMissingReviewEmailsAsync(new[] { review });
        return View(review);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "Reviews", new { area = "Admin", id });

        var review = await _bll.FindReviewAsync(id);
        if (review == null) return NotFound();
        if (review.UserId != User.GetUserId()) return NotFound();

        var removed = await _mediator.Send(new DeleteReviewCommand(id, User.GetUserId()));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRestaurantOptionsAsync(ReviewCreateViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = (await GetSelectableRestaurantsAsync())
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();
    }

    private async Task PopulateRestaurantOptionsAsync(ReviewEditViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = (await GetSelectableRestaurantsAsync())
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();
    }

    private async Task PopulateMissingReviewEmailsAsync(IEnumerable<ReviewDto> reviews)
    {
        var list = reviews.ToList();
        if (list.Count == 0) return;

        var missingUserIds = list
            .Where(r => string.IsNullOrWhiteSpace(r.AppUser?.Email))
            .Select(r => r.UserId)
            .Distinct()
            .ToList();

        if (missingUserIds.Count == 0) return;

        var usersById = (await _identity.GetUsersAsync())
            .Where(u => missingUserIds.Contains(u.Id))
            .ToDictionary(u => u.Id);

        foreach (var review in list)
        {
            if (!string.IsNullOrWhiteSpace(review.AppUser?.Email)) continue;
            if (!usersById.TryGetValue(review.UserId, out var user)) continue;

            review.AppUser ??= new ReviewUserDto { Id = review.UserId };
            review.AppUser.Email = !string.IsNullOrWhiteSpace(user.Email) ? user.Email : user.UserName;
        }
    }

    private async Task<List<RestaurantDto>> GetSelectableRestaurantsAsync()
    {
        var restaurants = (await _bll.GetRestaurantsAsync()).ToList();
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId()))
        {
            return restaurants;
        }

        var adminRestaurantIds = await _identity.GetAdminRestaurantIdsAsync(User.GetUserId());
        return restaurants.Where(r => adminRestaurantIds.Contains(r.Id)).ToList();
    }

    private async Task<bool> IsRestaurantSelectionAllowedAsync(Guid restaurantId)
    {
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId()))
        {
            return true;
        }

        return await _identity.IsRestaurantAdminAsync(User.GetUserId(), restaurantId);
    }
}
