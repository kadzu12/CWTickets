using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketAPI.Models;
using static TicketAPI.Controllers.ConcertsController;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly TicketDbContext _context;
        public ReviewController(
            TicketDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetUserReviews(int userId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.IdConcertNavigation)
                .ThenInclude(c => c.IdArtistNavigation)
                .Where(r => r.IdUser == userId)
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetReview(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.IdUserNavigation)
                .Include(r => r.IdConcertNavigation)
                .FirstOrDefaultAsync(r => r.IdReview == id);

            if (review == null)
            {
                return NotFound();
            }

            var reviewDto = new ReviewDto
            {
                IdReview = review.IdReview,
                IdUser = review.IdUser,
                IdConcert = review.IdConcert,
                TextReview = review.TextReview,
                RatingReview = review.RatingReview,
                DateReview = review.DateReview
            };

            return Ok(reviewDto);
        }
        [HttpGet("artist/{artistId}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetArtistReviews(int artistId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.IdConcertNavigation)
                .Include(r => r.IdUserNavigation)
                .Where(r => r.IdConcertNavigation.IdArtist == artistId)
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpPost]
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Проверяем, есть ли уже отзыв от этого пользователя на этот концерт
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.IdUser == createReviewDto.IdUser && r.IdConcert == createReviewDto.IdConcert);

            if (existingReview != null)
            {
                return Conflict("Вы уже оставляли отзыв на этот концерт");
            }

            var review = new Review
            {
                IdUser = createReviewDto.IdUser,
                IdConcert = createReviewDto.IdConcert,
                TextReview = createReviewDto.TextReview,
                RatingReview = createReviewDto.RatingReview,
                DateReview = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var reviewDto = new ReviewDto
            {
                IdReview = review.IdReview,
                IdUser = review.IdUser,
                IdConcert = review.IdConcert,
                TextReview = review.TextReview,
                RatingReview = review.RatingReview,
                DateReview = review.DateReview
            };

            return CreatedAtAction(nameof(GetReview), new { id = review.IdReview }, reviewDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewDto reviewDto)
        {
            if (id != reviewDto.IdReview)
            {
                return BadRequest();
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            review.TextReview = reviewDto.TextReview;
            review.RatingReview = reviewDto.RatingReview;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.IdReview == id);
        }
        public class ReviewDto
        {
            public int IdReview { get; set; }
            public int IdUser { get; set; }
            public int IdConcert { get; set; }
            public string TextReview { get; set; }
            public int? RatingReview { get; set; }
            public DateTime? DateReview { get; set; }
        }

        public class CreateReviewDto
        {
            public int IdUser { get; set; }
            public int IdConcert { get; set; }
            public string TextReview { get; set; }
            public int? RatingReview { get; set; }
        }
    }
}
