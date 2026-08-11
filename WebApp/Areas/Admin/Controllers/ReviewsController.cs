using Base.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Areas.Admin.ViewModels.Review;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;

namespace WebApp.Areas.Admin.Controllers;

public class ReviewsController : AdminBaseController
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

    public async Task<IActionResult> Index()
    {
        var reviews = (await _bll.GetReviewsAsync()).ToList();
        await PopulateMissingReviewEmailsAsync(reviews);
        return View(reviews);
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var review = await _bll.FindReviewAsync(id.Value);
        if (review == null) return NotFound();

        await PopulateMissingReviewEmailsAsync(new[] { review });
        return View(review);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new ReviewCreateViewModel();
        await PopulateRestaurantOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewCreateViewModel vm)
    {
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
        if (id == null) return NotFound();

        var review = await _bll.FindReviewAsync(id.Value);
        if (review == null) return NotFound();

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
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindReviewAsync(id);
        if (entity == null) return NotFound();

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
                    entity.UserId));
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
        if (id == null) return NotFound();

        var review = await _bll.FindReviewAsync(id.Value);
        if (review == null) return NotFound();

        await PopulateMissingReviewEmailsAsync(new[] { review });
        return View(review);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var review = await _bll.FindReviewAsync(id);
        if (review == null) return NotFound();

        var removed = await _mediator.Send(new DeleteReviewCommand(review.Id));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRestaurantOptionsAsync(ReviewCreateViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = await BuildRestaurantOptionsAsync(selectedRestaurantId);
    }

    private async Task PopulateRestaurantOptionsAsync(ReviewEditViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = await BuildRestaurantOptionsAsync(selectedRestaurantId);
    }

    private async Task<List<SelectListItem>> BuildRestaurantOptionsAsync(Guid? selectedRestaurantId = null)
    {
        return (await GetSelectableRestaurantsAsync())
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
