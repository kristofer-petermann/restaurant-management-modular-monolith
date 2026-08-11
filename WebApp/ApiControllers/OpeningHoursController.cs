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
    public class OpeningHoursController : ApiControllerBase
    {
        private readonly IRestaurantsModuleApi _bll;
        private readonly IMediator _mediator;
        private readonly OpeningHourPublicDTOv1Mapper _mapper = new();

        public OpeningHoursController(IRestaurantsModuleApi bll, IMediator mediator)
        {
            _bll = bll;
            _mediator = mediator;
        }

        // GET: api/OpeningHours
        [ProducesResponseType(typeof(IEnumerable<OpeningHourResponse>), 200)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OpeningHourResponse>>> GetOpeningHours()
        {
            var openingHours = (await _bll.GetOpeningHourReadsAsync()).ToList();
            if (!IsRoot() && await IsRestaurantAdminAnywhereAsync(_bll))
            {
                var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_bll);
                openingHours = openingHours.Where(oh => adminRestaurantIds.Contains(oh.RestaurantId)).ToList();
            }

            return openingHours.Select(oh => _mapper.Map(oh)!).ToList();
        }

        // GET: api/OpeningHours/5
        [ProducesResponseType(typeof(OpeningHourResponse), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<OpeningHourResponse>> GetOpeningHour(Guid id)
        {
            var openingHour = await _bll.FindOpeningHourReadAsync(id);

            if (openingHour == null)
            {
                return NotFound();
            }

            if (!IsRoot() && await IsRestaurantAdminAnywhereAsync(_bll))
            {
                var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_bll);
                if (!adminRestaurantIds.Contains(openingHour.RestaurantId))
                {
                    return NotFound();
                }
            }

            return _mapper.Map(openingHour)!;
        }

        // PUT: api/OpeningHours/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOpeningHour(Guid id, OpeningHourUpdateRequest openingHour)
        {
            if (id != openingHour.Id)
            {
                return BadRequest();
            }

            var entity = await _bll.FindOpeningHourAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            if (!await HasRestaurantAdminRightsAsync(_bll, entity.RestaurantId) ||
                !await HasRestaurantAdminRightsAsync(_bll, openingHour.RestaurantId))
            {
                return NotFound();
            }

            var updated = await _mediator.Send(new UpdateOpeningHourCommand(
                entity.Id,
                openingHour.RestaurantId,
                openingHour.DayOfWeek,
                openingHour.OpenTime,
                openingHour.CloseTime,
                openingHour.Summary));
            if (updated == null) return NotFound();

            return NoContent();
        }

        // POST: api/OpeningHours
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(typeof(OpeningHourResponse), 201)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<OpeningHourResponse>> PostOpeningHour(OpeningHourCreateRequest openingHour)
        {
            if (!await HasRestaurantAdminRightsAsync(_bll, openingHour.RestaurantId))
            {
                return NotFound();
            }

            var entity = await _mediator.Send(new CreateOpeningHourCommand(
                openingHour.RestaurantId,
                openingHour.DayOfWeek,
                openingHour.OpenTime,
                openingHour.CloseTime,
                string.IsNullOrWhiteSpace(openingHour.Summary) ? string.Empty : openingHour.Summary));

            return CreatedAtAction("GetOpeningHour", new { id = entity.Id }, _mapper.Map(entity)!);
        }

        // DELETE: api/OpeningHours/5
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOpeningHour(Guid id)
        {
            var openingHour = await _bll.FindOpeningHourAsync(id);
            if (openingHour == null)
            {
                return NotFound();
            }

            if (!await HasRestaurantAdminRightsAsync(_bll, openingHour.RestaurantId))
            {
                return NotFound();
            }

            var removed = await _mediator.Send(new DeleteOpeningHourCommand(openingHour.Id));
            if (!removed) return NotFound();

            return NoContent();
        }

    }
}
