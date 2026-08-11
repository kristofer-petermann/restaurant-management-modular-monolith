using Microsoft.AspNetCore.Mvc;
using WebApp.ApiContracts.v1;
using WebApp.ApiContracts.v1.Mappers;
using Base.Helpers;
using Shared.Contracts.Orders;
using Shared.Contracts.Orders.Commands;

namespace WebApp.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ApiControllerBase
    {
        private readonly IOrdersModuleApi _bll;
        private readonly IIdentityModuleApi _identity;
        private readonly OrderPublicDTOv1Mapper _mapper = new();

        public OrdersController(IOrdersModuleApi bll, IIdentityModuleApi identity)
        {
            _bll = bll;
            _identity = identity;
        }

        // GET: api/Orders
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), 200)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders()
        {
            var orders = (await _bll.GetOrdersAsync()).ToList();
            if (!IsRoot())
            {
                if (await IsRestaurantAdminAnywhereAsync(_identity))
                {
                    var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_identity);
                    orders = orders.Where(o => adminRestaurantIds.Contains(o.RestaurantId)).ToList();
                }
                else
                {
                    var userId = User.GetUserId();
                    orders = orders.Where(o => o.UserId == userId).ToList();
                }
            }

            return orders.Select(o => _mapper.Map(o)!).ToList();
        }

        // GET: api/Orders/5
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponse>> GetOrder(Guid id)
        {
            var order = await _bll.GetOrderAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            if (!await CanReadOrderAsync(_bll, _identity, order.Id))
            {
                return NotFound();
            }

            return _mapper.Map(order)!;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(Guid id, OrderUpdateRequest order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            var entity = await _bll.GetOrderAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            if (!IsRoot() &&
                entity.UserId != User.GetUserId() &&
                !await _identity.IsRestaurantAdminAsync(User.GetUserId(), entity.RestaurantId))
            {
                return NotFound();
            }

            // For non-root users, keep the original owner unchanged.
            if (IsRoot())
            {
                entity.UserId = order.UserId;
            }
            entity.RestaurantId = order.RestaurantId;
            entity.OrderStatus = order.OrderStatus;
            entity.TotalPrice = order.TotalPrice;
            entity.CreatedAt = order.CreatedAt;
            entity.PickupTime = order.PickupTime;
            entity.Summary = order.Summary;

            await _bll.UpdateOrderDetailsAsync(entity);

            return NoContent();
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(typeof(OrderResponse), 201)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> PostOrder(OrderCreateRequest order)
        {
            if (order.Items.Count == 0)
            {
                return BadRequest("At least one order item is required.");
            }

            var boundUserId = IsRoot() ? order.UserId : User.GetUserId();

            try
            {
                var submitResult = await _bll.SubmitOrderAsync(new SubmitOrderCommand(
                    order.RestaurantId,
                    order.Items.Select(i => new SubmitOrderItemCommand(i.MenuItemId, i.Quantity)).ToList(),
                    order.PickupTime,
                    order.Summary), boundUserId);

                var createdOrder = await _bll.GetOrderAsync(submitResult.OrderId, boundUserId);
                if (createdOrder == null) return NotFound();

                return CreatedAtAction("GetOrder", new { id = createdOrder.Id }, _mapper.Map(createdOrder)!);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Orders/submit
        [ProducesResponseType(typeof(SubmitOrderResponse), 201)]
        [ProducesResponseType(400)]
        [HttpPost("submit")]
        public async Task<ActionResult<SubmitOrderResponse>> SubmitOrder(SubmitOrderRequest request)
        {
            if (request.Items.Count == 0)
            {
                return BadRequest("At least one order item is required.");
            }

            var boundUserId = IsRoot() ? request.UserId : User.GetUserId();

            try
            {
                var result = await _bll.SubmitOrderAsync(new SubmitOrderCommand(
                    request.RestaurantId,
                    request.Items.Select(i => new SubmitOrderItemCommand(i.MenuItemId, i.Quantity)).ToList(),
                    request.PickupTime,
                    request.Summary), boundUserId);

                return CreatedAtAction("GetOrder", new { id = result.OrderId }, new SubmitOrderResponse
                {
                    OrderId = result.OrderId,
                    TotalAmount = result.TotalAmount
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Orders/5
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var order = await _bll.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (!IsRoot() && order.UserId != User.GetUserId())
            {
                return NotFound();
            }

            await _bll.DeleteOrderAsync(id);

            return NoContent();
        }
    }
}
