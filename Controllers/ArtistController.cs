using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketAPI.Models;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtistController : ControllerBase
    {
        private readonly TicketDbContext _context;
        public ArtistController(
            TicketDbContext context)
        {
            _context = context;
        }


        [HttpGet("GetArtists")]
        public ActionResult<IEnumerable<Artist>> GetArtists()
        {
            try
            {
                var artists = _context.Artists
                    .Include(a => a.IdGenres)
                    .Include(a => a.Concerts)
                        .ThenInclude(c => c.IdHallNavigation)
                    .AsNoTracking()
                    .ToList();

                // Обработка путей к фото
                foreach (var artist in artists)
                {
                    if (!string.IsNullOrEmpty(artist.ProfilePhotoArtist) &&
                        !artist.ProfilePhotoArtist.StartsWith("/artistAssets/"))
                    {
                        artist.ProfilePhotoArtist = $"/artistAssets/{artist.ProfilePhotoArtist}";
                    }

                    if (!string.IsNullOrEmpty(artist.BackgroundPhotoArtist) &&
                        !artist.BackgroundPhotoArtist.StartsWith("/artistAssets/"))
                    {
                        artist.BackgroundPhotoArtist = $"/artistAssets/{artist.BackgroundPhotoArtist}";
                    }
                }
                GenerateGeners();
                return Ok(artists);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке артистов: {ex.Message}");
                return StatusCode(500, "Произошла ошибка при загрузке данных артистов");
            }
        }

        [HttpGet("GetArtistByCity/{city}")]
        public ActionResult<IEnumerable<Artist>> GetArtistByCity(string city)
        {
            try
            {
                // Находим артиста с указанным ID, включая связанные данные
                var artistList = _context.Artists
                    .Include(a => a.IdGenres)
                    .Include(a => a.Concerts).ThenInclude(c => c.IdHallNavigation)
                    .Where(a => a.Concerts.Any(c => c.IdHallNavigation.CityHall == city))
                    .AsNoTracking() // Для оптимизации, если не планируется изменение данных
                    .ToList();

                if (artistList == null)
                {
                    return NotFound($"Артисты с городом {city} не найдены");
                }

                foreach (var artist in artistList)
                {
                    if (!string.IsNullOrEmpty(artist.ProfilePhotoArtist) &&
                        !artist.ProfilePhotoArtist.StartsWith("/artistAssets/"))
                    {
                        artist.ProfilePhotoArtist = $"/artistAssets/{artist.ProfilePhotoArtist}";
                    }

                    if (!string.IsNullOrEmpty(artist.BackgroundPhotoArtist) &&
                        !artist.BackgroundPhotoArtist.StartsWith("/artistAssets/"))
                    {
                        artist.BackgroundPhotoArtist = $"/artistAssets/{artist.BackgroundPhotoArtist}";
                    }
                }

                return Ok(artistList);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при получении артиста по городу: {ex.Message}");
                return StatusCode(500, "Произошла ошибка на сервере");
            }
        }
        [HttpGet("GetArtistById/{id}")]
        public ActionResult<Artist> GetArtistById(int id)
        {
            try
            {
                // Находим артиста с указанным ID, включая связанные данные
                var artistList = _context.Artists
                    .Include(a => a.IdGenres)
                    .Include(a => a.Concerts).ThenInclude(c => c.IdHallNavigation) // Загружаем данные о площадке
                    .AsNoTracking() // Для оптимизации, если не планируется изменение данных
                    .ToList();
                var artist = artistList.FirstOrDefault(a => a.IdArtist == id);
                if (artist == null)
                {
                    return NotFound($"Артист с ID {id} не найден");
                }

                // Корректируем пути к изображениям
                if (!artist.ProfilePhotoArtist.StartsWith("/artistAssets/"))
                {
                    artist.ProfilePhotoArtist = $"/artistAssets/{artist.ProfilePhotoArtist}";
                }

                if (!artist.BackgroundPhotoArtist.StartsWith("/artistAssets/"))
                {
                    artist.BackgroundPhotoArtist = $"/artistAssets/{artist.BackgroundPhotoArtist}";
                }

                return Ok(artist);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при получении артиста: {ex.Message}");
                return StatusCode(500, "Произошла ошибка на сервере");
            }
        }

        [HttpGet("GetFilteredArtists")]
        public ActionResult<IEnumerable<Artist>> GetFilteredArtists(
            [FromQuery] string? city,
            [FromQuery] string? genre,
            [FromQuery] DateOnly? date,
            [FromQuery] string? searchQuery,
            [FromQuery] decimal? priceRange,
            [FromQuery] string? sortBy)
        {
            try
            {
                using var context = new TicketDbContext();

                // Базовый запрос с включением связанных данных
                var query = _context.Artists
                    .Include(a => a.IdGenres)
                    .Include(a => a.Concerts.Where(c => c.StatusConcert == "Событие планируется"))
                        .ThenInclude(c => c.IdHallNavigation)
                        .ThenInclude(h => h.Sections)
                    .AsNoTracking();


                // Применяем фильтры только если параметры не null
                if (!string.IsNullOrEmpty(city))
                {
                    query = query.Where(a => a.Concerts
                        .Any(c => c.IdHallNavigation.CityHall.ToLower() == city.ToLower()));
                }

                if (!string.IsNullOrEmpty(genre))
                {
                    query = query.Where(a => a.IdGenres
                        .Any(g => g.NameGenre.ToLower() == genre.ToLower()));
                }

                if (date.HasValue)
                {
                    query = query.Where(a => a.Concerts.Any() &&
                                             a.Concerts.Any(c => c.DateStartConcert == date.Value));
                }

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query = query.Where(a => a.NameArtist.ToLower()
                        .Contains(searchQuery.ToLower()));
                }

                if (priceRange.HasValue)
                {
                    query = query.Where(a => a.Concerts.Any() &&
                                             a.Concerts.Any(c => c.IdHallNavigation.Sections.Any() &&
                                                                 c.IdHallNavigation.Sections
                                                                     .Any(s => s.PriceSection <= priceRange)));
                }

                // Если ВСЕ параметры фильтрации null - возвращаем всех артистов
                bool allFiltersEmpty = string.IsNullOrEmpty(city) &&
                                       string.IsNullOrEmpty(genre) &&
                                       !date.HasValue &&
                                       string.IsNullOrEmpty(searchQuery) &&
                                       (!priceRange.HasValue || priceRange == 10000);

                if (allFiltersEmpty)
                {
                    // Оптимизация: если фильтров нет, можно использовать более простой запрос
                    query = _context.Artists
                    .Include(a => a.IdGenres)
                    .Include(a => a.Concerts.Where(c => c.StatusConcert == "Событие планируется"))
                        .ThenInclude(c => c.IdHallNavigation)
                        .ThenInclude(h => h.Sections)
                    .AsNoTracking();
                }

                // Материализуем запрос
                var artists = query.ToList();

                // Обработка изображений
                foreach (var artist in artists)
                {
                    artist.ProfilePhotoArtist = ProcessImagePath(artist.ProfilePhotoArtist);
                    artist.BackgroundPhotoArtist = ProcessImagePath(artist.BackgroundPhotoArtist);
                }

                return Ok(artists);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return StatusCode(500, $"Произошла ошибка: {ex.Message}");
            }
        }


        public static string ProcessImagePath(string? path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return path.StartsWith("/artistAssets/") ? path : $"/artistAssets/{path}";
        }

        // Контроллер ConcertsController.cs
        [HttpGet("GetConcertById/{id}")]
        public ActionResult<Concert> GetConcertById(int id)
        {
            try
            {
                var concert = _context.Concerts
                    .Include(c => c.IdHallNavigation).ThenInclude(h=>h.Sections)
                    .Include(c => c.IdArtistNavigation)
                    .FirstOrDefault(c => c.IdConcert == id);

                if (concert == null)
                {
                    return NotFound(new { Message = $"Концерт с ID {id} не найден" });
                }


                return Ok(concert);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // DTO классы
        //public class ConcertDto
        //{
        //    public int IdConcert { get; set; }
        //    public DateOnly DateStartConcert { get; set; }
        //    public TimeOnly TimeStartConcert { get; set; }
        //    public int? AgeLimitConcert { get; set; }
        //    public string StatusConcert { get; set; }
        //    public HallDto IdHallNavigation { get; set; }
        //    public ArtistShortDto IdArtistNavigation { get; set; }
        //}

        //public class HallDto
        //{
        //    public int IdHall { get; set; }
        //    public string NameHall { get; set; }
        //    public string CityHall { get; set; }
        //    public string StreetHall { get; set; }
        //    public string BuildingHall { get; set; }
        //}

        //public class ArtistShortDto
        //{
        //    public int IdArtist { get; set; }
        //    public string NameArtist { get; set; }
        //}
        public static void GenerateGeners()
        {
            var artists = Program.context.Artists
                .Include(a => a.IdGenres)
                .ToList();

            // Обработка путей к фото
            foreach (var artist in artists)
            {
                if (artist.IdGenres == null || !artist.IdGenres.Any())
                {
                    artist.ColorArtist = "#FFFFFF";
                    return;
                }

                int sumR = 0, sumG = 0, sumB = 0, count = 0;

                foreach (var genre in artist.IdGenres)
                {
                    if (string.IsNullOrEmpty(genre.ColorGenre)) continue;

                    string hex = genre.ColorGenre.Trim();
                    if (hex.StartsWith("#"))
                        hex = hex.Substring(1);

                    if (hex.Length == 6)
                    {
                        int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                        int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

                        sumR += r;
                        sumG += g;
                        sumB += b;
                        count++;
                    }
                }

                if (count > 0)
                {
                    int avgR = sumR / count;
                    int avgG = sumG / count;
                    int avgB = sumB / count;
                    artist.ColorArtist = $"#{avgR:X2}{avgG:X2}{avgB:X2}";
                }
                else
                {
                    artist.ColorArtist = "#FFFFFF";
                }
                Program.context.SaveChanges();
            }
        }

        public class ArtistGenresDTO
        {
            public Artist Artist { get; set; }
            public List<Genre> Genres { get; set; }
        }

        [HttpGet("GetArtistsGenres")]
        public ActionResult<IEnumerable<ArtistGenresDTO>> GetArtistsGenres()
        {
            var artists = _context.Artists.ToList();
            var artistGenresList = new List<ArtistGenresDTO>();

            foreach (var artist in artists)
            {
                var genres = _context.Genres
                    .Where(g => g.IdArtists.Any(a => a.IdArtist == artist.IdArtist))
                    .ToList();

                artistGenresList.Add(new ArtistGenresDTO
                {
                    Artist = artist,
                    Genres = genres
                });
            }

            return Ok(artistGenresList);
        }

        [HttpGet("GetArtistGenres/{artistId}")]
        public ActionResult<ArtistGenresDTO> GetArtistGenres(int artistId)
        {
            var artist = _context.Artists
                .FirstOrDefault(a => a.IdArtist == artistId);

            if (artist == null)
            {
                return NotFound("Артист не найден");
            }
                
            var genres = _context.Genres
                .Where(g => g.IdArtists.Any(a => a.IdArtist == artistId)) // Проверяем связь
                .ToList();
            Console.WriteLine(genres.Count().ToString());
            return Ok(genres);
        }

    }
}
