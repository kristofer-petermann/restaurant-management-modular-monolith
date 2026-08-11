using Microsoft.AspNetCore.Mvc;
using WebApp.Areas.Admin.ViewModels.Dashboard;

namespace WebApp.Areas.Admin.Controllers;

public class DashboardController : AdminBaseController
{
    private readonly IRestaurantsModuleApi _restaurants;
    private readonly IOrdersModuleApi _orders;

    public DashboardController(IRestaurantsModuleApi restaurants, IOrdersModuleApi orders)
    {
        _restaurants = restaurants;
        _orders = orders;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new AdminDashboardViewModel
        {
            PageTitle = "Admin Dashboard",
            Subtitle = "Operational overview for root-level management.",
            Stats = new List<AdminDashboardStatViewModel>
            {
                new() { Label = "Restaurants", Value = (await _restaurants.GetRestaurantsAsync()).Count().ToString(), Controller = "Restaurants" },
                new() { Label = "Menus", Value = (await _restaurants.GetMenusAsync()).Count().ToString(), Controller = "Menus" },
                new() { Label = "Menu Items", Value = (await _restaurants.GetMenuItemsAsync()).Count().ToString(), Controller = "MenuItems" },
                new() { Label = "Orders", Value = (await _orders.GetOrdersAsync()).Count().ToString(), Controller = "Orders" },
                new() { Label = "Payments", Value = (await _orders.GetPaymentsAsync()).Count().ToString(), Controller = "Payments" },
                new() { Label = "Reviews", Value = (await _restaurants.GetReviewsAsync()).Count().ToString(), Controller = "Reviews" }
            }
        };

        return View(vm);
    }
}
