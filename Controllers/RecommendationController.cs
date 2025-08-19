using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TicketAPI.Models;
using Newtonsoft.Json;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly TicketDbContext _context;
        private readonly ILogger<RecommendationSchedulerService> _logger;
        public RecommendationController(
            TicketDbContext context,
            ILogger<RecommendationSchedulerService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public static string ProcessImagePath(string? path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return path.StartsWith("/artistAssets/") ? path : $"/artistAssets/{path}";
        }
        [HttpGet("GetRecommendationForUser/{idUser}")]
        public ActionResult<IEnumerable<Recommendation>> GetRecommendationForUser(int idUser)
        {
            try
            {
                var recommendations = _context.Recommendations
                    .Where(r=>r.IdUser == idUser)
                    .Include(r=>r.IdArtistNavigation)
                        .ThenInclude(a => a.IdGenres)
                    .Include(r=>r.IdArtistNavigation)
                        .ThenInclude(a => a.Concerts)
                        .ThenInclude(c => c.IdHallNavigation)
                    .AsNoTracking()
                    .ToList();

                // Обработка путей к фото
                foreach (var recommendation in recommendations)
                {
                    recommendation.IdArtistNavigation.ProfilePhotoArtist = ProcessImagePath(recommendation.IdArtistNavigation.ProfilePhotoArtist);
                    recommendation.IdArtistNavigation.BackgroundPhotoArtist = ProcessImagePath(recommendation.IdArtistNavigation.BackgroundPhotoArtist);
                }
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке рекомендаций пользователя: {ex.Message}");
                return StatusCode(500, "Произошла ошибка при загрузке данных рекомендаций пользователя");
            }
        }

        public class PopularArtistsDTO
        {
            public int id_artist { get; set; }
            public string name_artist { get; set; }
            public double popularity_score { get; set; }
        }

        [HttpGet("PopularArtists")]
        public ActionResult<IEnumerable<PopularArtistsDTO>> PopularArtists()
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Recommendation", "RecommendationProject", "popular_artists.json");

                if (!System.IO.File.Exists(path))
                {
                    _logger.LogWarning("Файл popular_artists.json не найден по пути: {Path}", path);
                    return NotFound("Данные о популярных исполнителях временно недоступны");
                }

                var json = System.IO.File.ReadAllText(path);

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogError("Файл popular_artists.json пуст");
                    return StatusCode(500, "Ошибка обработки данных");
                }

                var popularArtists = JsonConvert.DeserializeObject<List<PopularArtistsDTO>>(json);

                if (popularArtists == null || !popularArtists.Any())
                {
                    _logger.LogWarning("Не удалось десериализовать данные или список артистов пуст");
                    return NotFound("Нет данных о популярных исполнителях");
                }

                // Сортируем по убыванию популярности
                var sortedArtists = popularArtists
                    .OrderByDescending(a => a.popularity_score)
                    .ToList();

                return Ok(sortedArtists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении популярных артистов");
                return StatusCode(500, "Произошла внутренняя ошибка сервера");
            }
        }
    }
}
