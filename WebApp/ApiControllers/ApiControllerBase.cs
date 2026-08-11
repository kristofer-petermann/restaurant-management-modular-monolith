using Base.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Identity;

namespace WebApp.ApiControllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected bool IsRoot()
    {
        return User.IsInRole("root");
    }

    protected async Task<bool> HasRestaurantMembershipAsync(IIdentityModuleApi bll, Guid restaurantId)
    {
        if (IsRoot()) return true;

        _ = bll;
        var userId = User.GetUserId();
        var mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
        return await mediator.Send(new HasRestaurantMembershipQuery(userId, restaurantId));
    }
    
    protected async Task<bool> HasRestaurantMembershipAsync(IRestaurantsModuleApi bll, Guid restaurantId)
    {
        if (IsRoot()) return true;

        _ = bll;
        var userId = User.GetUserId();
        var mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
        return await mediator.Send(new HasRestaurantMembershipQuery(userId, restaurantId));
    }

    protected async Task<bool> HasRestaurantAdminRightsAsync(IIdentityModuleApi bll, Guid restaurantId)
    {
        if (IsRoot()) return true;

        return await bll.IsRestaurantAdminAsync(User.GetUserId(), restaurantId);
    }
    
    protected async Task<bool> HasRestaurantAdminRightsAsync(IRestaurantsModuleApi bll, Guid restaurantId)
    {
        if (IsRoot()) return true;

        _ = bll;
        var mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
        return await mediator.Send(new ValidateRestaurantUserPermissionQuery(User.GetUserId(), restaurantId, "admin"));
    }

    protected async Task<bool> IsRestaurantAdminAnywhereAsync(IIdentityModuleApi bll)
    {
        if (IsRoot()) return true;

        return await bll.IsRestaurantAdminAsync(User.GetUserId());
    }
    
    protected async Task<bool> IsRestaurantAdminAnywhereAsync(IRestaurantsModuleApi bll)
    {
        if (IsRoot()) return true;

        _ = bll;
        var mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
        var ids = await mediator.Send(new GetAdminRestaurantIdsQuery(User.GetUserId()));
        return ids.Count > 0;
    }

    protected async Task<List<Guid>> GetAdminRestaurantIdsAsync(IIdentityModuleApi bll)
    {
        return await bll.GetAdminRestaurantIdsAsync(User.GetUserId());
    }
    
    protected async Task<List<Guid>> GetAdminRestaurantIdsAsync(IRestaurantsModuleApi bll)
    {
        _ = bll;
        var mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
        return (await mediator.Send(new GetAdminRestaurantIdsQuery(User.GetUserId()))).ToList();
    }

    protected async Task<bool> CanReadOrderAsync(IOrdersModuleApi bll, IIdentityModuleApi identity, Guid orderId)
    {
        if (IsRoot()) return true;

        var userId = User.GetUserId();
        var order = await bll.GetOrderAsync(orderId);
        if (order == null) return false;

        if (await IsRestaurantAdminAnywhereAsync(identity))
        {
            return await HasRestaurantAdminRightsAsync(identity, order.RestaurantId);
        }

        return order.UserId == userId;
    }
}
