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
    public class AddressesController : ApiControllerBase
    {
        private readonly IRestaurantsModuleApi _bll;
        private readonly IMediator _mediator;
        private readonly AddressPublicDTOv1Mapper _mapper = new();

        public AddressesController(IRestaurantsModuleApi bll, IMediator mediator)
        {
            _bll = bll;
            _mediator = mediator;
        }

        // GET: api/Addresses
        [ProducesResponseType(typeof(IEnumerable<AddressResponse>), 200)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AddressResponse>>> GetAddresses()
        {
            var addresses = IsRoot()
                ? (await _bll.GetAddressReadsAsync()).ToList()
                : (await _bll.GetAddressReadsAsync(User.GetUserId())).ToList();
            return addresses.Select(a => _mapper.Map(a)!).ToList();
        }

        // GET: api/Addresses/5
        [ProducesResponseType(typeof(AddressResponse), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<AddressResponse>> GetAddress(Guid id)
        {
            var address = await _bll.FindAddressReadAsync(id);

            if (address == null)
            {
                return NotFound();
            }

            if (!await CanAccessAddressAsync(address))
            {
                return NotFound();
            }

            return _mapper.Map(address)!;
        }

        // PUT: api/Addresses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAddress(Guid id, AddressUpdateRequest address)
        {
            if (id != address.Id)
            {
                return BadRequest();
            }

            var entity = await _bll.FindAddressAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            if (!await CanAccessAddressEntityAsync(entity))
            {
                return NotFound();
            }

            var targetUserId = address.UserId;
            var targetRestaurantId = address.RestaurantId;

            if (!await ValidateAddressScopeAsync(targetUserId, targetRestaurantId))
            {
                return NotFound();
            }

            var updated = await _mediator.Send(new UpdateAddressCommand(
                entity.Id,
                address.Street,
                address.City,
                address.PostalCode,
                address.Country,
                address.Summary,
                IsRoot() ? targetUserId : (targetUserId.HasValue ? User.GetUserId() : null),
                targetRestaurantId));
            if (updated == null) return NotFound();

            return NoContent();
        }

        // POST: api/Addresses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(typeof(AddressResponse), 201)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<AddressResponse>> PostAddress(AddressCreateRequest address)
        {
            var targetUserId = address.UserId;
            var targetRestaurantId = address.RestaurantId;

            if (!await ValidateAddressScopeAsync(targetUserId, targetRestaurantId))
            {
                return NotFound();
            }

            var entity = await _mediator.Send(new CreateAddressCommand(
                address.Street,
                address.City,
                address.PostalCode,
                address.Country,
                string.IsNullOrWhiteSpace(address.Summary) ? string.Empty : address.Summary,
                IsRoot() ? targetUserId : (targetUserId.HasValue ? User.GetUserId() : null),
                targetRestaurantId));

            return CreatedAtAction("GetAddress", new { id = entity.Id }, _mapper.Map(entity)!);
        }

        // DELETE: api/Addresses/5
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            var address = await _bll.FindAddressAsync(id);
            if (address == null)
            {
                return NotFound();
            }

            if (!await CanAccessAddressEntityAsync(address))
            {
                return NotFound();
            }

            var removed = await _mediator.Send(new DeleteAddressCommand(address.Id));
            if (!removed) return NotFound();

            return NoContent();
        }

        private async Task<bool> CanAccessAddressAsync(Shared.Contracts.Restaurants.AddressReadDto address)
        {
            if (IsRoot()) return true;

            var userId = User.GetUserId();
            if (address.UserId == userId) return true;
            if (address.RestaurantId.HasValue)
            {
                return await HasRestaurantMembershipAsync(_bll, address.RestaurantId.Value);
            }

            return false;
        }

        private async Task<bool> CanAccessAddressEntityAsync(AddressDto address)
        {
            if (IsRoot()) return true;

            var userId = User.GetUserId();
            if (address.UserId == userId) return true;
            if (address.RestaurantId.HasValue)
            {
                return await HasRestaurantMembershipAsync(_bll, address.RestaurantId.Value);
            }

            return false;
        }

        private async Task<bool> ValidateAddressScopeAsync(Guid? userId, Guid? restaurantId)
        {
            if (!userId.HasValue && !restaurantId.HasValue) return false;
            if (IsRoot()) return true;

            var currentUserId = User.GetUserId();
            if (userId.HasValue && userId.Value != currentUserId) return false;
            if (restaurantId.HasValue && !await HasRestaurantMembershipAsync(_bll, restaurantId.Value)) return false;

            return true;
        }

    }
}
