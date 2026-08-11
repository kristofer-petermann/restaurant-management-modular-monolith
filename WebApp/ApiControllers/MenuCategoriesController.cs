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
    public class MenuCategoriesController : ApiControllerBase
    {
        private readonly IRestaurantsModuleApi _bll;
        private readonly IMediator _mediator;
        private readonly MenuCategoryPublicDTOv1Mapper _mapper = new();

        public MenuCategoriesController(IRestaurantsModuleApi bll, IMediator mediator)
        {
            _bll = bll;
            _mediator = mediator;
        }

        // GET: api/MenuCategories
        [ProducesResponseType(typeof(IEnumerable<MenuCategoryResponse>), 200)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuCategoryResponse>>> GetMenuCategories()
        {
            var menuCategories = (await _bll.GetMenuCategoryReadsAsync()).ToList();
            if (!IsRoot() && await IsRestaurantAdminAnywhereAsync(_bll))
            {
                var allowedMenuIds = await GetAdminMenuIdsAsync();
                menuCategories = menuCategories.Where(mc => allowedMenuIds.Contains(mc.MenuId)).ToList();
            }

            return menuCategories.Select(mc => _mapper.Map(mc)!).ToList();
        }

        // GET: api/MenuCategories/5
        [ProducesResponseType(typeof(MenuCategoryResponse), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuCategoryResponse>> GetMenuCategory(Guid id)
        {
            var menuCategory = await _bll.FindMenuCategoryReadAsync(id);

            if (menuCategory == null)
            {
                return NotFound();
            }

            var restaurantId = await GetRestaurantIdForMenuAsync(menuCategory.MenuId);
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

            return _mapper.Map(menuCategory)!;
        }

        // PUT: api/MenuCategories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenuCategory(Guid id, MenuCategoryUpdateRequest menuCategory)
        {
            if (id != menuCategory.Id)
            {
                return BadRequest();
            }

            var entity = await _bll.FindMenuCategoryAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var currentRestaurantId = await GetRestaurantIdForMenuAsync(entity.MenuId);
            var targetRestaurantId = await GetRestaurantIdForMenuAsync(menuCategory.MenuId);
            if (currentRestaurantId == null || targetRestaurantId == null ||
                !await HasRestaurantAdminRightsAsync(_bll, currentRestaurantId.Value) ||
                !await HasRestaurantAdminRightsAsync(_bll, targetRestaurantId.Value))
            {
                return NotFound();
            }

            var updated = await _mediator.Send(new UpdateMenuCategoryCommand(
                entity.Id,
                menuCategory.MenuId,
                menuCategory.Name,
                menuCategory.Summary));
            if (updated == null) return NotFound();

            return NoContent();
        }

        // POST: api/MenuCategories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(typeof(MenuCategoryResponse), 201)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<MenuCategoryResponse>> PostMenuCategory(MenuCategoryCreateRequest menuCategory)
        {
            var restaurantId = await GetRestaurantIdForMenuAsync(menuCategory.MenuId);
            if (restaurantId == null || !await HasRestaurantAdminRightsAsync(_bll, restaurantId.Value))
            {
                return NotFound();
            }

            var entity = await _mediator.Send(new CreateMenuCategoryCommand(
                menuCategory.MenuId,
                menuCategory.Name,
                string.IsNullOrWhiteSpace(menuCategory.Summary) ? string.Empty : menuCategory.Summary));

            return CreatedAtAction("GetMenuCategory", new { id = entity.Id }, _mapper.Map(entity)!);
        }

        // DELETE: api/MenuCategories/5
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuCategory(Guid id)
        {
            var menuCategory = await _bll.FindMenuCategoryAsync(id);
            if (menuCategory == null)
            {
                return NotFound();
            }

            var restaurantId = await GetRestaurantIdForMenuAsync(menuCategory.MenuId);
            if (restaurantId == null || !await HasRestaurantAdminRightsAsync(_bll, restaurantId.Value))
            {
                return NotFound();
            }

            var removed = await _mediator.Send(new DeleteMenuCategoryCommand(menuCategory.Id));
            if (!removed) return NotFound();

            return NoContent();
        }

        private async Task<Guid?> GetRestaurantIdForMenuAsync(Guid menuId)
        {
            return (await _bll.FindMenuAsync(menuId))?.RestaurantId;
        }

        private async Task<HashSet<Guid>> GetAdminMenuIdsAsync()
        {
            var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_bll);
            return (await _bll.GetMenusAsync())
                .Where(m => adminRestaurantIds.Contains(m.RestaurantId))
                .Select(m => m.Id)
                .ToHashSet();
        }
    }
}
