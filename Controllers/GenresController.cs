using Microsoft.AspNetCore.Mvc;
using TicketAPI.Models;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenresController : ControllerBase
    {
        private readonly TicketDbContext _context;
        public GenresController(
            TicketDbContext context)
        {
            _context = context;
        }
        [HttpGet("GetGenres")]
        public ActionResult<IEnumerable<Genre>> GetGenres()
        {
            var genres = _context.Genres.ToList();
            //foreach (var genre in genres)
            //{
            //    if (!string.IsNullOrEmpty(genre.IconUrlGenre) &&
            //        !genre.IconUrlGenre.StartsWith("/artistAssets/"))
            //    {
            //        genre.IconUrlGenre = $"/artistAssets/{genre.IconUrlGenre}";
            //    }
            //}
            return Ok(genres);
        }
    }
}
