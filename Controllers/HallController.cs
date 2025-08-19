using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketAPI.Models;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HallController : ControllerBase
    {
        private readonly TicketDbContext _context;
        public HallController(
            TicketDbContext context)
        {
            _context = context;
        }
        [HttpGet("GetCities")]
        public ActionResult<IEnumerable<string>> GetCities()
        {
            var cities = _context.Halls
                .Select(h => h.CityHall)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return Ok(cities);
        }
        [HttpGet("GetHallsByCity")]
        public ActionResult<IEnumerable<Hall>> GetHallsByCity([FromQuery] string city)
        {
            try
            {
                var halls = _context.Halls
                    .Where(h => h.CityHall.ToLower() == city.ToLower())
                    .AsNoTracking()
                    .ToList();

                return Ok(halls);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении залов: {ex.Message}");
            }
        }
        [HttpGet("GetHalls")]
        public ActionResult<IEnumerable<Hall>> GetHalls()
        {
            try
            {
                var halls = _context.Halls
                    .AsNoTracking()
                    .ToList();

                return Ok(halls);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении залов: {ex.Message}");
            }
        }


        [HttpGet("GetHallSections/{hallId}")]
        public ActionResult<IEnumerable<HallSectionDto>> GetHallSections(int hallId)
        {
            try
            {
                var sections = _context.Sections
                    .Where(s => s.IdHall == hallId)
                    .OrderBy(s => s.PriceSection)
                    .Select(s => new HallSectionDto
                    {
                        IdSection = s.IdSection,
                        IdHall = s.IdHall,
                        NameSection = s.NameSection,
                        SchemaSection = s.SchemaSection,
                        TotalSeatsSection = s.TotalSeatsSection,
                        PriceSection = s.PriceSection,
                        TypeSection = s.TypeSection,
                        UnitCountSection = s.UnitCountSection,
                        SeatsPerUnitSection = s.SeatsPerUnitSection
                    })
                    .ToList();

                if (sections == null || sections.Count == 0)
                {
                    return NotFound($"Секции для зала с ID {hallId} не найдены");
                }

                return Ok(sections);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении секций зала: {ex.Message}");
            }
        }
        public class HallSectionDto
        {
            public int IdSection { get; set; }
            public int IdHall { get; set; }
            public string NameSection { get; set; }
            public string SchemaSection { get; set; }
            public int? TotalSeatsSection { get; set; }
            public decimal? PriceSection { get; set; }

            public string? TypeSection { get; set; }
            public int? UnitCountSection { get; set; }
            public int? SeatsPerUnitSection { get; set; }
        }
    }
}

// DTO класс для секции зала
//public class HallSectionDto
//{
//    public int IdSection { get; set; }
//    public int IdHall { get; set; }
//    public string NameSection { get; set; }
//    public string SchemeSection { get; set; }
//    public int? SeatsCount { get; set; }
//    public decimal? Price { get; set; }
//}

