using Microsoft.AspNetCore.Mvc;
using WebApp.ApiContracts.v1;
using WebApp.ApiContracts.v1.Mappers;
using Base.Helpers;
using MediatR;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;

namespace WebApp.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantsController : ApiControllerBase
    {
        private readonly IRestaurantsModuleApi _bll;
        private readonly IMediator _mediator;
        private readonly RestaurantPublicDTOv1Mapper _mapper = new();

        public RestaurantsController(IRestaurantsModuleApi bll, IMediator mediator)
        {
            _bll = bll;
            _mediator = mediator;
        }

        // GET: api/Restaurants
        [ProducesResponseType(typeof(IEnumerable<RestaurantResponse>), 200)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestaurantResponse>>> GetRestaurants()
        {
            var restaurants = (await _bll.GetRestaurantReadsAsync()).ToList();
            if (!IsRoot() && await IsRestaurantAdminAnywhereAsync(_bll))
            {
                var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_bll);
                restaurants = restaurants.Where(r => adminRestaurantIds.Contains(r.Id)).ToList();
            }

            return restaurants.Select(r => _mapper.Map(r)!).ToList();
        }

        // GET: api/Restaurants/5
        [ProducesResponseType(typeof(RestaurantResponse), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantResponse>> GetRestaurant(Guid id)
        {
            var restaurant = await _bll.FindRestaurantReadAsync(id);

            if (restaurant == null)
            {
                return NotFound();
            }

            if (!IsRoot() && await IsRestaurantAdminAnywhereAsync(_bll))
            {
                var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_bll);
                if (!adminRestaurantIds.Contains(restaurant.Id))
                {
                    return NotFound();
                }
            }

            return _mapper.Map(restaurant)!;
        }

        // PUT: api/Restaurants/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRestaurant(Guid id, RestaurantUpdateRequest restaurant)
        {
            if (id != restaurant.Id)
            {
                return BadRequest();
            }

            var entity = await _bll.FindRestaurantAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            if (!await HasRestaurantAdminRightsAsync(_bll, entity.Id))
            {
                return NotFound();
            }

            var updated = await _mediator.Send(new UpdateRestaurantCommand(
                entity.Id,
                restaurant.RestaurantName,
                restaurant.Description,
                string.IsNullOrWhiteSpace(restaurant.Phone) ? null : restaurant.Phone,
                string.IsNullOrWhiteSpace(restaurant.Email) ? null : restaurant.Email,
                restaurant.Summary));
            if (updated == null) return NotFound();

            return NoContent();
        }

        // POST: api/Restaurants
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(typeof(RestaurantResponse), 201)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<RestaurantResponse>> PostRestaurant(RestaurantCreateRequest restaurant)
        {
            var entity = await _mediator.Send(new CreateRestaurantCommand(
                restaurant.RestaurantName,
                restaurant.Description,
                string.IsNullOrWhiteSpace(restaurant.Phone) ? null : restaurant.Phone,
                string.IsNullOrWhiteSpace(restaurant.Email) ? null : restaurant.Email,
                string.IsNullOrWhiteSpace(restaurant.Summary) ? null : restaurant.Summary));

            return CreatedAtAction("GetRestaurant", new { id = entity.Id }, _mapper.Map(entity)!);
        }

        // DELETE: api/Restaurants/5
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestaurant(Guid id)
        {
            var restaurant = await _bll.FindRestaurantAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }

            if (!await HasRestaurantAdminRightsAsync(_bll, restaurant.Id))
            {
                return NotFound();
            }

            var removed = await _mediator.Send(new DeleteRestaurantCommand(restaurant.Id));
            if (!removed) return NotFound();

            return NoContent();
        }

    }
}
