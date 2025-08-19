using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TicketAPI.Models;
using TicketAPI.Services;
using static TicketAPI.Controllers.AuthController;

namespace TicketAPI.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly TicketDbContext _context;

        public UsersController(TicketDbContext context)
        {
            _context = context;
        }
        public class RegisterDto
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string BirthDate { get; set; }
            public string Login { get; set; }
            public string Email { get; set; }
            public string City { get; set; }
            public string Password { get; set; }
        }
        [HttpPost("register")]
        public ActionResult<User> Register([FromBody] RegisterDto request)
        {
            try
            {
                var existingUser = Program.context.Users.FirstOrDefault(u => u.EmailUser == request.Email);
                if (existingUser != null)
                {
                    return Conflict("Пользователь с такой почтой уже существует.");
                }

                // Хэшируем пароль
                AuthService.CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

                var newUser = new User
                {
                    FirstNameUser = request.FirstName,
                    LastNameUser = request.LastName,
                    BirthDateUser = DateOnly.Parse(request.BirthDate),
                    LoginUser = request.Login,
                    EmailUser = request.Email,
                    CityUser = request.City,
                    PasswordHashUser = Convert.ToBase64String(passwordHash),
                    PasswordSaltUser = Convert.ToBase64String(passwordSalt),
                    IdRole = 3 // Роль по умолчанию — пользователь
                };

                Program.context.Users.Add(newUser);
                Program.context.SaveChanges();
                return StatusCode(201, newUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка регистрации: {ex.Message} {ex.InnerException?.Message}");
            }
        }
        [HttpPut("UpdateUser")]
        public ActionResult<User> UpdateUser([FromBody] UpdateDto request)
        {
            try
            {
                if (request == null || request.Id <= 0)
                    return BadRequest("Неверные данные запроса");

                if (string.IsNullOrEmpty(request.Email))
                    return BadRequest("Email обязателен");

                var contextUser = _context.Users.FirstOrDefault(u => u.IdUser == request.Id);
                if (contextUser == null)
                    return NotFound("Пользователь не найден");

                var existingUser = _context.Users
                    .FirstOrDefault(u => u.EmailUser == request.Email && u.IdUser != request.Id);

                if (existingUser != null)
                    return Conflict("Пользователь с такой почтой уже существует");

                contextUser.FirstNameUser = request.FirstName ?? contextUser.FirstNameUser;
                contextUser.LastNameUser = request.LastName ?? contextUser.LastNameUser;
                contextUser.EmailUser = request.Email;

                if (DateOnly.TryParse(request.BirthDate, out var birthDate))
                    contextUser.BirthDateUser = birthDate;

                contextUser.CityUser = request.City ?? contextUser.CityUser;

                _context.SaveChanges();

                return Ok(contextUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка обновления: {ex.Message}");
            }
        }

        public class UpdateDto
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string BirthDate { get; set; }
            public string Email { get; set; }
            public string City { get; set; }
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(UserLoginDto request)
        {
            var user = Program.context.Users
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefault(u => u.EmailUser == request.Email);

            if (user == null || !AuthService.VerifyPasswordHash(request.Password, user.PasswordHashUser, user.PasswordSaltUser))
            {
                return NotFound("Неверные учетные данные");
            }

            return Ok(user);
        }

        public class UserLoginDto
        {
            public string Email { get; set; } = null!;
            public string Password { get; set; } = null!;
        }
        [HttpGet("GetTicketsUser/{userId}")]
        public ActionResult<IEnumerable<Ticket>> GetTicketsUser(int userId)
        {
            var now = DateTime.Now;

            var tickets = _context.Tickets
                .Where(t => t.IdUser == userId)
                .Include(u => u.IdSectionNavigation)
                .Include(t => t.IdConcertNavigation)
                    .ThenInclude(c => c.IdArtistNavigation)
                .ToList();

            foreach (var ticket in tickets)
            {
                if (ticket.StatusTicket == "Активен")
                {
                    if (ticket.IdConcertNavigation.DateStartConcert < DateOnly.FromDateTime(now) ||
                        (ticket.IdConcertNavigation.DateStartConcert == DateOnly.FromDateTime(now) &&
                         ticket.IdConcertNavigation.TimeStartConcert < TimeOnly.FromDateTime(now)))
                    {
                        ticket.StatusTicket = "Использован";
                    }
                    else if (ticket.IdConcertNavigation.StatusConcert == "Событие отменено")
                    {
                        ticket.StatusTicket = "Возвращен";
                    }
                }
            }

            _context.SaveChanges();

            return Ok(tickets);
        }

        [HttpPost("CancelTicket/{ticketId}")]
        public async Task<IActionResult> CancelTicket(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.IdConcertNavigation)
                .FirstOrDefaultAsync(t => t.IdTicket == ticketId);

            if (ticket == null)
            {
                return NotFound("Билет не найден");
            }

            var now = DateTime.Now;
            var concertDate = new DateTime(
                ticket.IdConcertNavigation.DateStartConcert.Year,
                ticket.IdConcertNavigation.DateStartConcert.Month,
                ticket.IdConcertNavigation.DateStartConcert.Day);

            var concertTime = ticket.IdConcertNavigation.TimeStartConcert;

            if (concertDate < now.Date ||
                (concertDate == now.Date && concertTime < TimeOnly.FromDateTime(now)))
            {
                return BadRequest("Нельзя отменить билет на прошедший концерт");
            }

            if (ticket.IdConcertNavigation.StatusConcert == "Событие отменено")
            {
                return BadRequest("Концерт отменен, билет автоматически возвращен");
            }

            if (ticket.StatusTicket != "Активен")
            {
                return BadRequest("Можно отменить только активные билеты");
            }

            ticket.StatusTicket = "Возвращен";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Билет успешно отменен",
                TicketId = ticket.IdTicket
            });
        }

        [HttpGet("GetFavoritesUser/{userId}")]
        public ActionResult<Ticket> GetFavoritesUser(int userId)
        {
            var favorites = Program.context.Favorites.Where(f => f.IdUser == userId).Include(f=>f.IdArtistNavigation);
            return Ok(favorites);
        }
    }
}