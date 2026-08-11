using Microsoft.AspNetCore.Mvc;
using WebApp.ApiContracts.v1;
using WebApp.ApiContracts.v1.Mappers;
using Base.Helpers;
using Shared.Contracts.Orders;

namespace WebApp.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ApiControllerBase
    {
        private readonly IOrdersModuleApi _bll;
        private readonly IIdentityModuleApi _identity;
        private readonly PaymentPublicDTOv1Mapper _mapper = new();

        public PaymentsController(IOrdersModuleApi bll, IIdentityModuleApi identity)
        {
            _bll = bll;
            _identity = identity;
        }

        // GET: api/Payments
        [ProducesResponseType(typeof(IEnumerable<PaymentResponse>), 200)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetPayments()
        {
            var payments = (await _bll.GetPaymentsAsync()).ToList();
            if (!IsRoot())
            {
                var orders = (await _bll.GetOrdersAsync()).ToList();
                if (await IsRestaurantAdminAnywhereAsync(_identity))
                {
                    var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_identity);
                    var orderIds = orders.Where(o => adminRestaurantIds.Contains(o.RestaurantId)).Select(o => o.Id).ToHashSet();
                    payments = payments.Where(p => orderIds.Contains(p.OrderId)).ToList();
                }
                else
                {
                    var userId = User.GetUserId();
                    var orderIds = orders.Where(o => o.UserId == userId).Select(o => o.Id).ToHashSet();
                    payments = payments.Where(p => orderIds.Contains(p.OrderId)).ToList();
                }
            }

            return payments.Select(p => _mapper.Map(p)!).ToList();
        }

        // GET: api/Payments/5
        [ProducesResponseType(typeof(PaymentResponse), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentResponse>> GetPayment(Guid id)
        {
            var payment = await _bll.FindPaymentAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            if (!await CanReadOrderAsync(_bll, _identity, payment.OrderId))
            {
                return NotFound();
            }

            return _mapper.Map(payment)!;
        }

        // PUT: api/Payments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPayment(Guid id, PaymentUpdateRequest payment)
        {
            if (id != payment.Id)
            {
                return BadRequest();
            }

            var entity = await _bll.FindPaymentAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            if (!await CanAccessOrderAsync(entity.OrderId) || !await CanAccessOrderAsync(payment.OrderId))
            {
                return NotFound();
            }

            entity.OrderId = payment.OrderId;
            entity.Amount = payment.Amount;
            entity.PaymentStatus = payment.PaymentStatus;
            entity.PaymentMethod = payment.PaymentMethod;
            entity.PaidAt = payment.PaidAt;
            entity.Summary = payment.Summary;

            await _bll.UpdatePaymentDetailsAsync(entity);

            return NoContent();
        }

        // POST: api/Payments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(typeof(PaymentResponse), 201)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<PaymentResponse>> PostPayment(PaymentCreateRequest payment)
        {
            if (!await CanAccessOrderAsync(payment.OrderId))
            {
                return NotFound();
            }

            var entity = new PaymentDto
            {
                Id = Guid.NewGuid(),
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentStatus = payment.PaymentStatus,
                PaymentMethod = payment.PaymentMethod,
                PaidAt = payment.PaidAt,
                Summary = string.IsNullOrWhiteSpace(payment.Summary) ? string.Empty : payment.Summary
            };

            await _bll.CreatePaymentAsync(entity);

            return CreatedAtAction("GetPayment", new { id = entity.Id }, _mapper.Map(entity)!);
        }

        // DELETE: api/Payments/5
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(Guid id)
        {
            var payment = await _bll.FindPaymentAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            if (!await CanAccessOrderAsync(payment.OrderId))
            {
                return NotFound();
            }

            await _bll.DeletePaymentAsync(payment.Id);

            return NoContent();
        }

        private async Task<bool> CanAccessOrderAsync(Guid orderId)
        {
            if (IsRoot()) return true;

            var userId = User.GetUserId();
            var order = await _bll.GetOrderAsync(orderId);
            return order != null &&
                   (order.UserId == userId || await _identity.IsRestaurantAdminAsync(userId, order.RestaurantId));
        }
    }
}
