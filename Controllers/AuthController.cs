using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TicketAPI.Models;
using TicketAPI.Services;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        //private readonly AuthService _authService;
        //private readonly IConfiguration _configuration;

        //public AuthController(AuthService authService, IConfiguration configuration)
        //{
        //    _authService = authService;
        //    _configuration = configuration;
        //}


        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] RegisterDto request)
        //{
        //    try
        //    {
        //        //if (!ModelState.IsValid)
        //        //{
        //        //    return BadRequest(ModelState);
        //        //}

        //        //if (await _authService.UserExists(request.Email))
        //        //{
        //        //    return BadRequest("User already exists");
        //        //}

        //        var user = new User
        //        {
        //            FirstNameUser = request.FirstName,
        //            LastNameUser = request.LastName,
        //            BirthDateUser = DateOnly.Parse(request.BirthDate),
        //            LoginUser = request.Login,
        //            EmailUser = request.Email,
        //            CityUser = request.City,
        //            PasswordHashUser = request.Password,
        //            PasswordSaltUser = request.Password,
        //            IdRole = 3
        //        };
        //        Program.context.Users.Add(user);
        //        Program.context.SaveChanges();
        //        //var registeredUser = await _authService.Register(user, request.Password);

        //        return Ok(user);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        //    }
        //}

        //[HttpPost("login")]
        //public async Task<ActionResult<User>> Login(UserLoginDto request)
        //{
        //    //var user = await _authService.Login(request.Email, request.Password);
        //    var user = Program.context.Users.Where(u => u.EmailUser == request.Email && u.PasswordHashUser == request.Password);
        //    if (user == null)
        //    {
        //        return Unauthorized("Invalid credentials");
        //    }

        //    //var token = _authService.CreateToken(user);
        //    return Ok(user);
        //}

        //    [HttpGet("me"), Authorize]
        //    public async Task<ActionResult<User>> GetMe()
        //    {
        //        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //        var user = await _authService.GetUserById(userId);
        //        return Ok(user);
        //    }
        //}

        public class RegisterDto
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string BirthDate { get; set; } // <-- changed
            public string Login { get; set; }
            public string Email { get; set; }
            public string City { get; set; }
            public string Password { get; set; }
        }

        public class UserLoginDto
        {
            public string Email { get; set; } = null!;
            public string Password { get; set; } = null!;
        }
    }
}
