using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketAPI.Models;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteController : ControllerBase
    {
        private readonly TicketDbContext _context;
        public FavoriteController(
            TicketDbContext context)
        {
            _context = context;
        }
        private string FormatPhotoPath(string photoPath)
        {
            if (!string.IsNullOrEmpty(photoPath) &&
                !photoPath.StartsWith("/artistAssets/"))
            {
                return $"/artistAssets/{photoPath}";
            }
            return photoPath;
        }
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Favorite>>> GetUserFavorites(int userId)
        {
            var favorites = _context.Favorites
                .Where(f => f.IdUser == userId).Include(f => f.IdArtistNavigation).ThenInclude(a => a.IdGenres);
                
            foreach (var favorite in favorites)
            {
                favorite.IdArtistNavigation.ProfilePhotoArtist = FormatPhotoPath(favorite.IdArtistNavigation.ProfilePhotoArtist);
                favorite.IdArtistNavigation.BackgroundPhotoArtist = FormatPhotoPath(favorite.IdArtistNavigation.BackgroundPhotoArtist);
            }
            return favorites.ToList();
        }

        [HttpPost("addFavorite")]
        public async Task<ActionResult<Favorite>> AddFavorite([FromBody] FavoriteDto favoriteDto)
        {
            // Проверка существования пользователя
            var user = await Program.context.Users.FindAsync(favoriteDto.idUser);
            if (user == null)
            {
                return BadRequest($"Пользователь с ID {favoriteDto.idUser} не найден");
            }

            // Проверка существования артиста
            var artist = await _context.Artists.FindAsync(favoriteDto.idArtist);
            if (artist == null)
            {
                return BadRequest($"Артист с ID {favoriteDto.idArtist} не найден");
            }

            // Проверка, не добавлен ли уже артист в избранное
            var exists = await _context.Favorites
                .AnyAsync(f => f.IdUser == favoriteDto.idUser && f.IdArtist == favoriteDto.idArtist);

            if (exists)
            {
                return BadRequest("Этот артист уже в избранном");
            }

            var favorite = new Favorite
            {
                IdUser = favoriteDto.idUser,
                IdArtist = favoriteDto.idArtist,
                AddedDateFavorite = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(favorite);
        }

        [HttpDelete("removeFavorite/{id}")]
        public async Task<IActionResult> RemoveFavorite(int id)
        {
            var favorite = await _context.Favorites.FindAsync(id);
            if (favorite == null)
            {
                return NotFound();
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("getFavorite{id}")]
        public async Task<ActionResult<Favorite>> GetFavorite(int id)
        {
            var favorite = await _context.Favorites.FindAsync(id);
            if (favorite == null)
            {
                return NotFound();
            }
            return favorite;
        }
    }

    public class FavoriteDto
    {
        public int idUser { get; set; }
        public int idArtist { get; set; }
    }
}
