using Microsoft.AspNetCore.Mvc;
using WebApp.ApiContracts.v1;
using WebApp.ApiContracts.v1.Mappers;
using Base.Helpers;
using Shared.Contracts.Orders;

namespace WebApp.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderRowsController : ApiControllerBase
    {
        private readonly IOrdersModuleApi _bll;
        private readonly IIdentityModuleApi _identity;
        private readonly OrderRowPublicDTOv1Mapper _mapper = new();

        public OrderRowsController(IOrdersModuleApi bll, IIdentityModuleApi identity)
        {
            _bll = bll;
            _identity = identity;
        }

        // GET: api/OrderRows
        [ProducesResponseType(typeof(IEnumerable<OrderRowResponse>), 200)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderRowResponse>>> GetOrderRows()
        {
            var orderRows = (await _bll.GetOrderRowsAsync()).ToList();
            if (!IsRoot())
            {
                var orders = (await _bll.GetOrdersAsync()).ToList();
                if (await IsRestaurantAdminAnywhereAsync(_identity))
                {
                    var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_identity);
                    var orderIds = orders.Where(o => adminRestaurantIds.Contains(o.RestaurantId)).Select(o => o.Id).ToHashSet();
                    orderRows = orderRows.Where(or => orderIds.Contains(or.OrderId)).ToList();
                }
                else
                {
                    var userId = User.GetUserId();
                    var orderIds = orders.Where(o => o.UserId == userId).Select(o => o.Id).ToHashSet();
                    orderRows = orderRows.Where(or => orderIds.Contains(or.OrderId)).ToList();
                }
            }

            return orderRows.Select(or => _mapper.Map(or)!).ToList();
        }

        // GET: api/OrderRows/5
        [ProducesResponseType(typeof(OrderRowResponse), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderRowResponse>> GetOrderRow(Guid id)
        {
            var orderRow = await _bll.FindOrderRowAsync(id);

            if (orderRow == null)
            {
                return NotFound();
            }

            if (!await CanReadOrderAsync(_bll, _identity, orderRow.OrderId))
            {
                return NotFound();
            }

            return _mapper.Map(orderRow)!;
        }

        // PUT: api/OrderRows/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderRow(Guid id, OrderRowUpdateRequest orderRow)
        {
            if (id != orderRow.Id)
            {
                return BadRequest();
            }

            var entity = await _bll.FindOrderRowAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            if (!await CanAccessOrderAsync(entity.OrderId) || !await CanAccessOrderAsync(orderRow.OrderId))
            {
                return NotFound();
            }

            entity.OrderId = orderRow.OrderId;
            entity.MenuItemId = orderRow.MenuItemId;
            entity.Quantity = orderRow.Quantity;
            entity.UnitPrice = orderRow.UnitPrice;
            entity.Summary = orderRow.Summary;

            await _bll.UpdateOrderRowDetailsAsync(entity);

            return NoContent();
        }

        // POST: api/OrderRows
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(typeof(OrderRowResponse), 201)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<OrderRowResponse>> PostOrderRow(OrderRowCreateRequest orderRow)
        {
            if (!await CanAccessOrderAsync(orderRow.OrderId))
            {
                return NotFound();
            }

            var entity = new OrderRowDto
            {
                Id = Guid.NewGuid(),
                OrderId = orderRow.OrderId,
                MenuItemId = orderRow.MenuItemId,
                Quantity = orderRow.Quantity,
                UnitPrice = orderRow.UnitPrice,
                Summary = string.IsNullOrWhiteSpace(orderRow.Summary) ? string.Empty : orderRow.Summary
            };

            await _bll.CreateOrderRowAsync(entity);

            return CreatedAtAction("GetOrderRow", new { id = entity.Id }, _mapper.Map(entity)!);
        }

        // DELETE: api/OrderRows/5
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderRow(Guid id)
        {
            var orderRow = await _bll.FindOrderRowAsync(id);
            if (orderRow == null)
            {
                return NotFound();
            }

            if (!await CanAccessOrderAsync(orderRow.OrderId))
            {
                return NotFound();
            }

            await _bll.DeleteOrderRowAsync(orderRow.Id);

            return NoContent();
        }

        private async Task<bool> CanAccessOrderAsync(Guid orderId)
        {
            if (IsRoot()) return true;

            var order = await _bll.GetOrderAsync(orderId);
            return order != null && order.UserId == User.GetUserId();
        }
    }
}
