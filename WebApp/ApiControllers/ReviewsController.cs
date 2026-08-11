using Microsoft.AspNetCore.Mvc;
using WebApp.ApiContracts.v1;
using WebApp.ApiContracts.v1.Mappers;
using Base.Helpers;
using MediatR;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;
using Shared.Contracts.Restaurants.Results;

namespace WebApp.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ApiControllerBase
    {
        private readonly IRestaurantsModuleApi _bll;
        private readonly IMediator _mediator;
        private readonly ReviewPublicDTOv1Mapper _mapper = new();

        public ReviewsController(IRestaurantsModuleApi bll, IMediator mediator)
        {
            _bll = bll;
            _mediator = mediator;
        }

        // GET: api/Reviews
        [ProducesResponseType(typeof(IEnumerable<ReviewResponse>), 200)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetReviews()
        {
            var reviews = (await _bll.GetReviewReadsAsync()).ToList();
            if (!IsRoot() && await IsRestaurantAdminAnywhereAsync(_bll))
            {
                var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_bll);
                reviews = reviews.Where(r => adminRestaurantIds.Contains(r.RestaurantId)).ToList();
            }

            return reviews.Select(r => _mapper.Map(r)!).ToList();
        }

        // GET: api/Reviews/5
        [ProducesResponseType(typeof(ReviewResponse), 200)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewResponse>> GetReview(Guid id)
        {
            var review = await _bll.FindReviewReadAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            if (!IsRoot() && await IsRestaurantAdminAnywhereAsync(_bll))
            {
                var adminRestaurantIds = await GetAdminRestaurantIdsAsync(_bll);
                if (!adminRestaurantIds.Contains(review.RestaurantId))
                {
                    return NotFound();
                }
            }

            return _mapper.Map(review)!;
        }

        // PUT: api/Reviews/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(Guid id, ReviewUpdateRequest review)
        {
            if (id != review.Id)
            {
                return BadRequest();
            }

            if (review.RestaurantId == Guid.Empty)
            {
                return BadRequest("RestaurantId is required.");
            }

            var entity = await _bll.FindReviewAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            if (!IsRoot() && entity.UserId != User.GetUserId())
            {
                return NotFound();
            }

            if (await IsRestaurantAdminAnywhereAsync(_bll) &&
                !await HasRestaurantAdminRightsAsync(_bll, review.RestaurantId))
            {
                return Forbid();
            }

            var boundUserId = IsRoot() ? review.UserId : User.GetUserId();

            entity.UserId = boundUserId;
            entity.RestaurantId = review.RestaurantId;
            entity.Rating = review.Rating;
            entity.Comment = string.IsNullOrWhiteSpace(review.Comment) ? null : review.Comment;
            entity.CreatedAt = review.CreatedAt;
            entity.Summary = review.Summary;

            try
            {
                var updated = await _mediator.Send(new UpdateReviewCommand(
                    entity.Id,
                    entity.RestaurantId,
                    entity.UserId,
                    entity.Rating,
                    entity.Comment,
                    entity.CreatedAt,
                    entity.Summary));
                if (updated == null) return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        // POST: api/Reviews
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(typeof(ReviewResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [HttpPost]
        public async Task<ActionResult<ReviewResponse>> PostReview(ReviewCreateRequest review)
        {
            if (review.RestaurantId == Guid.Empty)
            {
                return BadRequest("RestaurantId is required.");
            }

            Guid boundUserId;
            bool isAdminAnywhere;
            try
            {
                boundUserId = IsRoot() ? review.UserId : User.GetUserId();
                isAdminAnywhere = await IsRestaurantAdminAnywhereAsync(_bll);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }

            if (isAdminAnywhere &&
                !await HasRestaurantAdminRightsAsync(_bll, review.RestaurantId))
            {
                return Forbid();
            }

            PublishReviewResult result;
            try
            {
                result = await _bll.PublishReviewAsync(new PublishReviewCommand(
                    review.RestaurantId,
                    review.Rating,
                    review.Comment,
                    review.Summary), boundUserId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            var created = await _bll.FindReviewAsync(result.ReviewId, boundUserId);
            if (created == null) return NotFound();

            return CreatedAtAction("GetReview", new { id = created.Id }, _mapper.Map(created)!);
        }

        // DELETE: api/Reviews/5
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var review = await _bll.FindReviewAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            if (!IsRoot() && review.UserId != User.GetUserId())
            {
                return NotFound();
            }

            var removed = await _mediator.Send(new DeleteReviewCommand(review.Id));
            if (!removed) return NotFound();

            return NoContent();
        }

    }
}
