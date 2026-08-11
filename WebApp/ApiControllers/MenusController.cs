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
    public class MenusController : ApiControllerBase
    {
        private readonly IRestaurantsModuleApi _bll;
        private readonly IMediator _mediator;
        private readonly MenuPublicDTOv1Mapper _mapper = new();

        public MenusController(IRestaurantsModuleApi bll, IMediator mediator)
        {
            _bll = bll;
            _mediator = mediator;
        }

        // GET: api/Menus
        [ProducesResponseType(typeof(IEnumerable<MenuResponse>), 200)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuResponse>>> GetMenus()
        {
            var menus = (await _bll.GetMenuReadsAsync()).ToList();
            if (!IsRoot() && await IsRestaurantAdminAnywhereAsync(_bll))
            {
                var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_bll);
                menus = menus.Where(m => adminRestaurantIds.Contains(m.RestaurantId)).ToList();
            }

            return menus.Select(m => _mapper.Map(m)!).ToList();
        }

        // GET: api/Menus/5
        [ProducesResponseType(typeof(MenuResponse), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuResponse>> GetMenu(Guid id)
        {
            var menu = await _bll.FindMenuReadAsync(id);

            if (menu == null)
            {
                return NotFound();
            }

            if (!IsRoot() && await IsRestaurantAdminAnywhereAsync(_bll))
            {
                var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_bll);
                if (!adminRestaurantIds.Contains(menu.RestaurantId))
                {
                    return NotFound();
                }
            }

            return _mapper.Map(menu)!;
        }

        // PUT: api/Menus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenu(Guid id, MenuUpdateRequest menu)
        {
            if (id != menu.Id)
            {
                return BadRequest();
            }

            var entity = await _bll.FindMenuAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            if (!await HasRestaurantAdminRightsAsync(_bll, entity.RestaurantId) ||
                !await HasRestaurantAdminRightsAsync(_bll, menu.RestaurantId))
            {
                return NotFound();
            }

            var updated = await _mediator.Send(new UpdateMenuCommand(
                entity.Id,
                menu.RestaurantId,
                menu.Name,
                menu.IsActive,
                menu.Summary));
            if (updated == null) return NotFound();

            return NoContent();
        }

        // POST: api/Menus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(typeof(MenuResponse), 201)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<MenuResponse>> PostMenu(MenuCreateRequest menu)
        {
            if (!await HasRestaurantAdminRightsAsync(_bll, menu.RestaurantId))
            {
                return NotFound();
            }

            var entity = await _mediator.Send(new CreateMenuCommand(
                menu.RestaurantId,
                menu.Name,
                menu.IsActive,
                string.IsNullOrWhiteSpace(menu.Summary) ? string.Empty : menu.Summary));

            return CreatedAtAction("GetMenu", new { id = entity.Id }, _mapper.Map(entity)!);
        }

        // DELETE: api/Menus/5
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(Guid id)
        {
            var menu = await _bll.FindMenuAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            if (!await HasRestaurantAdminRightsAsync(_bll, menu.RestaurantId))
            {
                return NotFound();
            }

            var removed = await _mediator.Send(new DeleteMenuCommand(menu.Id));
            if (!removed) return NotFound();

            return NoContent();
        }

    }
}
