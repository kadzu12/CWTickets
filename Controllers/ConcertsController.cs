using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using TicketAPI.Models;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConcertsController : ControllerBase
    {
        private readonly TicketDbContext _context;
        public ConcertsController(
            TicketDbContext context)
        {
            _context = context;
        }
        public class ConcertDto
        {
            public int IdConcert { get; set; }
            public DateOnly DateStartConcert { get; set; }
            public TimeOnly TimeStartConcert { get; set; }
            public decimal MinPrice { get; set; }
            public int IdArtist { get; set; }
            public string NameArtist { get; set; }
            public string DescriptionArtist { get; set; }
            public string ProfilePhotoArtist { get; set; }
            public string BackgroundPhotoArtist { get; set; }
            public int IdHall { get; set; }
            public string NameHall { get; set; }
            public string CityHall { get; set; }
        }
        [HttpGet("GetConcerts")]
        public ActionResult<IEnumerable<ConcertDto>> GetConcerts()
        {
            try
            {
                var currentDate = DateTime.Now;

                // Обновляем статус в базе данных для прошедших концертов
                var pastConcerts = _context.Concerts
                    .Where(c => c.StatusConcert != "Событие отменено" &&
                           (c.DateStartConcert < DateOnly.FromDateTime(currentDate) ||
                           (c.DateStartConcert == DateOnly.FromDateTime(currentDate) &&
                            c.TimeStartConcert < TimeOnly.FromDateTime(currentDate)))).ToList();

                foreach (var concert in pastConcerts)
                {
                    concert.StatusConcert = "Событие прошло";
                    _context.Entry(concert).State = EntityState.Modified;
                }

                _context.SaveChanges(); // Используем SaveChangesAsync

                var concerts = _context.Concerts.Where(c=>c.StatusConcert == "Событие планируется")
                    .Include(c => c.IdArtistNavigation)
                    .Include(c => c.IdHallNavigation)
                        .ThenInclude(h => h.Sections)
                    .AsNoTracking()
                    .ToList();

                var result = concerts.Select(c => new ConcertDto
                {
                    IdConcert = c.IdConcert,
                    DateStartConcert = c.DateStartConcert,
                    TimeStartConcert = c.TimeStartConcert,

                    IdArtist = c.IdArtistNavigation.IdArtist,
                    NameArtist = c.IdArtistNavigation.NameArtist,
                    DescriptionArtist = c.IdArtistNavigation.DescriptionArtist,
                    ProfilePhotoArtist = FormatPhotoPath(c.IdArtistNavigation.ProfilePhotoArtist),
                    BackgroundPhotoArtist = FormatPhotoPath(c.IdArtistNavigation.BackgroundPhotoArtist),
                    IdHall = c.IdHallNavigation.IdHall,
                    NameHall = c.IdHallNavigation.NameHall,
                    CityHall = c.IdHallNavigation.CityHall,

                    MinPrice = c.IdHallNavigation.Sections.Any()
                        ? (decimal)c.IdHallNavigation.Sections.Min(s => s.PriceSection)
                        : 0
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке концертов: {ex.Message}");
                return StatusCode(500, "Произошла ошибка при загрузке данных концертов");
            }
        }
        [HttpGet("GetFilteredConcerts")]
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

                // Базовый запрос с включением связанных данных
                var query = _context.Concerts.Include(c => c.IdArtistNavigation)
                        .ThenInclude(a => a.IdGenres)
                    .Include(c => c.IdHallNavigation)
                        .ThenInclude(h => h.Sections)
                    .Where(c=>c.StatusConcert == "Событие планируется")
                    .AsNoTracking();


                // Применяем фильтры только если параметры не null
                if (!string.IsNullOrEmpty(city))
                {
                    query = query.Where(c => c.IdHallNavigation.CityHall.ToLower() == city.ToLower());
                }

                if (!string.IsNullOrEmpty(genre))
                {
                    query = query.Where(c => c.IdArtistNavigation.IdGenres
                        .Any(g => g.NameGenre.ToLower() == genre.ToLower()));
                }

                if (date.HasValue)
                {
                    query = query.Where(c => c.DateStartConcert == date.Value);
                }

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query = query.Where(c => c.IdArtistNavigation.NameArtist.ToLower()
                        .Contains(searchQuery.ToLower()));
                }

                if (priceRange.HasValue)
                {
                    query = query.Where(c => c.IdHallNavigation.Sections.Any() &&
                                                                 c.IdHallNavigation.Sections
                                                                     .Any(s => s.PriceSection <= priceRange));
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
                    query = _context.Concerts
                        .Include(c => c.IdArtistNavigation)
                            .ThenInclude(a => a.IdGenres)
                        .Include(c => c.IdHallNavigation)
                            .ThenInclude(h => h.Sections)
                        .Where(c => c.StatusConcert == "Событие планируется")
                    .AsNoTracking();
                }

                // Материализуем запрос
                var concerts = query.ToList();

                var result = concerts.Select(c => new ConcertDto
                {
                    IdConcert = c.IdConcert,
                    DateStartConcert = c.DateStartConcert,
                    TimeStartConcert = c.TimeStartConcert,

                    IdArtist = c.IdArtistNavigation.IdArtist,
                    NameArtist = c.IdArtistNavigation.NameArtist,
                    DescriptionArtist = c.IdArtistNavigation.DescriptionArtist,
                    ProfilePhotoArtist = FormatPhotoPath(c.IdArtistNavigation.ProfilePhotoArtist),
                    BackgroundPhotoArtist = FormatPhotoPath(c.IdArtistNavigation.BackgroundPhotoArtist),
                    IdHall = c.IdHallNavigation.IdHall,
                    NameHall = c.IdHallNavigation.NameHall,
                    CityHall = c.IdHallNavigation.CityHall,

                    MinPrice = c.IdHallNavigation.Sections.Any()
        ? (decimal)c.IdHallNavigation.Sections.Min(s => s.PriceSection)
        : 0
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return StatusCode(500, $"Произошла ошибка: {ex.Message}");
            }
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

        [HttpPut("UpdateConcert")]
        public async Task<IActionResult> UpdateConcert([FromBody] ConcertUpdateDto concertDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var concert = await _context.Concerts
                    .Include(c => c.IdArtistNavigation)
                    .Include(c => c.IdHallNavigation)
                    .Include(c => c.Tickets)
                    .FirstOrDefaultAsync(c => c.IdConcert == concertDto.IdConcert);

                if (concert == null)
                {
                    return NotFound($"Концерт с ID {concertDto.IdConcert} не найден");
                }

                // Проверка даты (нельзя установить на сегодня или прошедшую дату)
                if (!DateOnly.TryParse(concertDto.DateStartConcert, out var date) || date <= DateOnly.FromDateTime(DateTime.Today))
                {
                    return BadRequest("Дата концерта должна быть не раньше завтрашнего дня");
                }

                // Проверка на дублирование концерта артиста в эту дату
                var artistConcertExists = await _context.Concerts
                    .AnyAsync(c => c.IdArtist == concert.IdArtist
                                && c.DateStartConcert == date
                                && c.IdConcert != concertDto.IdConcert);
                if (artistConcertExists)
                {
                    return BadRequest("У артиста уже есть концерт в эту дату");
                }

                // Проверка на занятость зала в эту дату
                var hallConcertExists = await _context.Concerts
                    .AnyAsync(c => c.IdHall == concertDto.IdHall
                                && c.DateStartConcert == date
                                && c.IdConcert != concertDto.IdConcert);
                if (hallConcertExists)
                {
                    return BadRequest("Зал уже занят в эту дату");
                }

                
        bool isCancelled = concert.StatusConcert != "Событие отменено" &&
                          concertDto.StatusConcert == "Событие отменено";

                // Обновление полей
                concert.IdHall = concertDto.IdHall;
                concert.StatusConcert = concertDto.StatusConcert;
                concert.DateStartConcert = date;

                if (TimeOnly.TryParse(concertDto.TimeStartConcert, out var time))
                {
                    concert.TimeStartConcert = time;
                }
                else
                {
                    return BadRequest("Неверный формат времени");
                }

                concert.AgeLimitConcert = Convert.ToInt32(concertDto.AgeLimitConcert);

                // Если концерт отменен, обновляем все билеты и отправляем уведомления
                if (isCancelled)
                {
                    foreach (var ticket in concert.Tickets)
                    {
                        ticket.StatusTicket = "Возвращен";

                        var message = $"Концерт {concert.IdArtistNavigation.NameArtist} отменён в {concert.IdHallNavigation.CityHall}. " +
                                      "Просим прощения за неудобство. Возврат средств в ближайшие пару дней.";
                        await NotificationController.CreateNotification(
                            ticket.IdUser,
                            message,
                            concert.IdArtist,
                            concert.IdConcert);
                    }
                }

                await _context.SaveChangesAsync();

                var userIds = await _context.Favorites
                    .Where(fa => fa.IdArtist == concert.IdArtist)
                    .Select(fa => fa.IdUser)
                    .Distinct()
                    .ToListAsync();

                // Отправляем уведомления об изменении концерта (если не отмена)
                if (!isCancelled)
                {
                    foreach (var userId in userIds)
                    {
                        var message = $"Изменена информация о концерте {concert.IdArtistNavigation.NameArtist}. " +
                                     $"{concert.DateStartConcert} в {concert.IdHallNavigation.NameHall}.";
                        await NotificationController.CreateNotification(
                            userId,
                            message,
                            concert.IdArtist,
                            concert.IdConcert);
                    }
                }

                return Ok(concert);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    title = "Ошибка при обновлении концерта",
                    detail = ex.Message,
                    status = 500
                });
            }

        }



        public class ConcertUpdateDto
        {
            public int IdConcert { get; set; }
            public int IdArtist { get; set; } 
            public int IdHall { get; set; }
            public string StatusConcert { get; set; }
            public string DateStartConcert { get; set; } 
            public string TimeStartConcert { get; set; }
            public string AgeLimitConcert { get; set; }
        }
        [HttpPost("AddConcert")]
        public async Task<IActionResult> AddConcert([FromBody] ConcertModerationDto concertDto)
        {
            try
            {
                // Проверяем существование артиста и зала
                var artistExists = await _context.Artists.AnyAsync(a => a.IdArtist == concertDto.IdArtist);
                var hallExists = await _context.Halls.AnyAsync(h => h.IdHall == concertDto.IdHall);

                if (!artistExists || !hallExists)
                {
                    return BadRequest("Артист или зал не найдены");
                }

                // Проверка даты (нельзя добавить на сегодня или прошедшую дату)
                if (concertDto.DateStartConcert <= DateOnly.FromDateTime(DateTime.Today))
                {
                    return BadRequest("Дата концерта должна быть не раньше завтрашнего дня");
                }

                // Проверка на дублирование концерта артиста в эту дату
                var artistConcertExists = await _context.Concerts
                    .AnyAsync(c => c.IdArtist == concertDto.IdArtist
                                && c.DateStartConcert == concertDto.DateStartConcert);
                if (artistConcertExists)
                {
                    return BadRequest("У артиста уже есть концерт в эту дату");
                }

                // Проверка на занятость зала в эту дату
                var hallConcertExists = await _context.Concerts
                    .AnyAsync(c => c.IdHall == concertDto.IdHall
                                && c.DateStartConcert == concertDto.DateStartConcert);
                if (hallConcertExists)
                {
                    return BadRequest("Зал уже занят в эту дату");
                }

                var concert = new Concert
                {
                    IdArtist = concertDto.IdArtist,
                    IdHall = concertDto.IdHall,
                    DateStartConcert = concertDto.DateStartConcert,
                    TimeStartConcert = concertDto.TimeStartConcert,
                    AgeLimitConcert = concertDto.AgeLimitConcert,
                    StatusConcert = concertDto.StatusConcert ?? "Событие планируется"
                };

                _context.Concerts.Add(concert);
                await _context.SaveChangesAsync();

                // Возвращаем созданный концерт с полными данными
                var result = await _context.Concerts
                    .Include(c => c.IdArtistNavigation)
                    .Include(c => c.IdHallNavigation)
                    .FirstOrDefaultAsync(c => c.IdConcert == concert.IdConcert);

                var userIds = await _context.Favorites
                    .Where(fa => fa.IdArtist == concert.IdArtist)
                    .Select(fa => fa.IdUser)
                    .Distinct()
                    .ToListAsync();

                foreach (var userId in userIds)
                {
                    var message = $"Добавлен новый концерт {concert.IdArtistNavigation.NameArtist}! " +
                                 $"{concert.DateStartConcert} в {concert.IdHallNavigation.NameHall}.";
                    await NotificationController.CreateNotification(
                        userId,
                        message,
                        concert.IdArtist,
                        concert.IdConcert);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при добавлении концерта: {ex.Message}");
            }
        }


        public class ArtistDto
        {
            public int IdArtist { get; set; }
            public string NameArtist { get; set; }
            public string DescriptionArtist { get; set; }
            public string ProfilePhotoArtist { get; set; }
            public string BackgroundPhotoArtist { get; set; }
        }

        public class HallDto
        {
            public int IdHall { get; set; }
            public string NameHall { get; set; }
            public string CityHall { get; set; }
        }
        public class ConcertModerationDto
        {
            public int IdArtist { get; set; }
            public int IdHall { get; set; }
            public DateOnly DateStartConcert { get; set; }
            public TimeOnly TimeStartConcert { get; set; }
            public int? AgeLimitConcert { get; set; }
            public string? StatusConcert { get; set; }
        }
    }
}
