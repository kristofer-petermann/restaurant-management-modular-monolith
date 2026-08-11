using Microsoft.AspNetCore.Mvc;
using WebApp.ApiContracts.v1;
using WebApp.ApiContracts.v1.Mappers;
using Base.Helpers;
using MediatR;
using Shared.Contracts.Identity;

namespace WebApp.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantUsersController : ApiControllerBase
    {
        private readonly IIdentityModuleApi _bll;
        private readonly IMediator _mediator;
        private readonly RestaurantUserPublicDTOv1Mapper _mapper = new();

        public RestaurantUsersController(IIdentityModuleApi bll, IMediator mediator)
        {
            _bll = bll;
            _mediator = mediator;
        }

        // GET: api/RestaurantUsers
        [ProducesResponseType(typeof(IEnumerable<RestaurantUserResponse>), 200)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestaurantUserResponse>>> GetRestaurantUsers()
        {
            var restaurantUsers = (await _mediator.Send(new GetRestaurantUsersQuery())).ToList();
            if (!IsRoot())
            {
                var userId = User.GetUserId();
                var allowedRestaurantIds = restaurantUsers
                    .Where(own => own.UserId == userId)
                    .Select(own => own.RestaurantId)
                    .ToHashSet();
                restaurantUsers = restaurantUsers
                    .Where(ru => allowedRestaurantIds.Contains(ru.RestaurantId))
                    .ToList();
            }

            return restaurantUsers.Select(ru => _mapper.Map(ru)!).ToList();
        }

        // GET: api/RestaurantUsers/5
        [ProducesResponseType(typeof(RestaurantUserResponse), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantUserResponse>> GetRestaurantUser(Guid id)
        {
            var restaurantUser = await _mediator.Send(new GetRestaurantUserByIdQuery(id));

            if (restaurantUser == null)
            {
                return NotFound();
            }

            if (!await CanReadRestaurantAsync(restaurantUser.RestaurantId))
            {
                return NotFound();
            }

            return _mapper.Map(restaurantUser)!;
        }

        // PUT: api/RestaurantUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRestaurantUser(Guid id, RestaurantUserUpdateRequest restaurantUser)
        {
            if (id != restaurantUser.Id)
            {
                return BadRequest();
            }

            var current = await _mediator.Send(new GetRestaurantUserByIdQuery(id));
            if (current == null)
            {
                return NotFound();
            }

            if (!await HasRestaurantAdminRightsAsync(_bll, current.RestaurantId) ||
                !await HasRestaurantAdminRightsAsync(_bll, restaurantUser.RestaurantId))
            {
                return NotFound();
            }
            var updated = await _mediator.Send(new UpdateRestaurantUserCommand(
                id,
                restaurantUser.RestaurantId,
                restaurantUser.UserId,
                (RestaurantUserRole)restaurantUser.UserRole,
                string.IsNullOrWhiteSpace(restaurantUser.Role) ? null : restaurantUser.Role,
                restaurantUser.Summary));
            if (updated == null) return NotFound();

            return NoContent();
        }

        // POST: api/RestaurantUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(typeof(RestaurantUserResponse), 201)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<RestaurantUserResponse>> PostRestaurantUser(RestaurantUserCreateRequest restaurantUser)
        {
            if (!await HasRestaurantAdminRightsAsync(_bll, restaurantUser.RestaurantId))
            {
                return NotFound();
            }

            var created = await _mediator.Send(new AssignRestaurantUserCommand(
                restaurantUser.RestaurantId,
                restaurantUser.UserId,
                (RestaurantUserRole)restaurantUser.UserRole,
                string.IsNullOrWhiteSpace(restaurantUser.Role) ? null : restaurantUser.Role,
                restaurantUser.Summary));

            return CreatedAtAction("GetRestaurantUser", new { id = created.Id }, _mapper.Map(created)!);
        }

        // DELETE: api/RestaurantUsers/5
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestaurantUser(Guid id)
        {
            var restaurantUser = await _mediator.Send(new GetRestaurantUserByIdQuery(id));
            if (restaurantUser == null)
            {
                return NotFound();
            }

            if (!await HasRestaurantAdminRightsAsync(_bll, restaurantUser.RestaurantId))
            {
                return NotFound();
            }

            await _mediator.Send(new RemoveRestaurantUserCommand(restaurantUser.Id));

            return NoContent();
        }

        private async Task<bool> CanReadRestaurantAsync(Guid restaurantId)
        {
            if (IsRoot()) return true;

            return await HasRestaurantMembershipAsync(_bll, restaurantId);
        }

    }
}
