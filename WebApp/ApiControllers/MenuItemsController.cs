using Microsoft.AspNetCore.Mvc;
using WebApp.ApiContracts.v1;
using WebApp.ApiContracts.v1.Mappers;
using MediatR;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;

namespace WebApp.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemsController : ApiControllerBase
    {
        private readonly IRestaurantsModuleApi _bll;
        private readonly IMediator _mediator;
        private readonly MenuItemPublicDTOv1Mapper _mapper = new();

        public MenuItemsController(IRestaurantsModuleApi bll, IMediator mediator)
        {
            _bll = bll;
            _mediator = mediator;
        }

        // GET: api/MenuItems
        [ProducesResponseType(typeof(IEnumerable<MenuItemResponse>), 200)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItemResponse>>> GetMenuItems()
        {
            var menuItems = (await _bll.GetMenuItemReadsAsync()).ToList();
            if (!IsRoot() && await IsRestaurantAdminAnywhereAsync(_bll))
            {
                var allowedCategoryIds = await GetAdminMenuCategoryIdsAsync();
                menuItems = menuItems.Where(mi => allowedCategoryIds.Contains(mi.MenuCategoryId)).ToList();
            }

            return menuItems.Select(mi => _mapper.Map(mi)!).ToList();
        }

        // GET: api/MenuItems/5
        [ProducesResponseType(typeof(MenuItemResponse), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuItemResponse>> GetMenuItem(Guid id)
        {
            var menuItem = await _bll.FindMenuItemReadAsync(id);

            if (menuItem == null)
            {
                return NotFound();
            }

            var restaurantId = await GetRestaurantIdForMenuCategoryAsync(menuItem.MenuCategoryId);
            if (restaurantId == null)
            {
                return NotFound();
            }

            if (!IsRoot() && await IsRestaurantAdminAnywhereAsync(_bll))
            {
                var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_bll);
                if (!adminRestaurantIds.Contains(restaurantId.Value))
                {
                    return NotFound();
                }
            }

            return _mapper.Map(menuItem)!;
        }

        // PUT: api/MenuItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenuItem(Guid id, MenuItemUpdateRequest menuItem)
        {
            if (id != menuItem.Id)
            {
                return BadRequest();
            }

            var entity = await _bll.FindMenuItemAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var currentRestaurantId = await GetRestaurantIdForMenuCategoryAsync(entity.MenuCategoryId);
            var targetRestaurantId = await GetRestaurantIdForMenuCategoryAsync(menuItem.MenuCategoryId);
            if (currentRestaurantId == null || targetRestaurantId == null ||
                !await HasRestaurantAdminRightsAsync(_bll, currentRestaurantId.Value) ||
                !await HasRestaurantAdminRightsAsync(_bll, targetRestaurantId.Value))
            {
                return NotFound();
            }

            var updated = await _mediator.Send(new UpdateMenuItemCommand(
                entity.Id,
                menuItem.MenuCategoryId,
                menuItem.Name,
                menuItem.Description,
                menuItem.Price,
                menuItem.IsAvailable,
                string.IsNullOrWhiteSpace(menuItem.ImageUrl) ? null : menuItem.ImageUrl,
                menuItem.Summary));
            if (updated == null) return NotFound();

            return NoContent();
        }

        // POST: api/MenuItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(typeof(MenuItemResponse), 201)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<MenuItemResponse>> PostMenuItem(MenuItemCreateRequest menuItem)
        {
            var restaurantId = await GetRestaurantIdForMenuCategoryAsync(menuItem.MenuCategoryId);
            if (restaurantId == null || !await HasRestaurantAdminRightsAsync(_bll, restaurantId.Value))
            {
                return NotFound();
            }

            var entity = await _mediator.Send(new CreateMenuItemCommand(
                menuItem.MenuCategoryId,
                menuItem.Name,
                string.IsNullOrWhiteSpace(menuItem.Description) ? string.Empty : menuItem.Description,
                menuItem.Price,
                menuItem.IsAvailable,
                string.IsNullOrWhiteSpace(menuItem.ImageUrl) ? null : menuItem.ImageUrl,
                string.IsNullOrWhiteSpace(menuItem.Summary) ? string.Empty : menuItem.Summary));

            return CreatedAtAction("GetMenuItem", new { id = entity.Id }, _mapper.Map(entity)!);
        }

        // DELETE: api/MenuItems/5
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(Guid id)
        {
            var menuItem = await _bll.FindMenuItemAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }

            var restaurantId = await GetRestaurantIdForMenuCategoryAsync(menuItem.MenuCategoryId);
            if (restaurantId == null || !await HasRestaurantAdminRightsAsync(_bll, restaurantId.Value))
            {
                return NotFound();
            }

            var removed = await _mediator.Send(new DeleteMenuItemCommand(menuItem.Id));
            if (!removed) return NotFound();

            return NoContent();
        }

        private async Task<Guid?> GetRestaurantIdForMenuCategoryAsync(Guid menuCategoryId)
        {
            var menuCategory = await _bll.FindMenuCategoryAsync(menuCategoryId);
            if (menuCategory == null) return null;

            return (await _bll.FindMenuAsync(menuCategory.MenuId))?.RestaurantId;
        }

        private async Task<HashSet<Guid>> GetAdminMenuCategoryIdsAsync()
        {
            var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_bll);
            var menuIds = (await _bll.GetMenusAsync())
                .Where(m => adminRestaurantIds.Contains(m.RestaurantId))
                .Select(m => m.Id)
                .ToHashSet();

            return (await _bll.GetMenuCategoriesAsync())
                .Where(mc => menuIds.Contains(mc.MenuId))
                .Select(mc => mc.Id)
                .ToHashSet();
        }
    }
}
