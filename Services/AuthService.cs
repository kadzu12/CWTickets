using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TicketAPI.Models;

namespace TicketAPI.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly TicketDbContext _context;

        public AuthService(IConfiguration configuration, TicketDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<User> Register(User user, string password)
        {
            // 1. Используем await для асинхронных операций
            // 2. Добавляем проверку на null
            if (user == null || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Invalid user data");
            }

            CreatePasswordHash(password, out var passwordHash, out var passwordSalt);

            user.PasswordHashUser = Convert.ToBase64String(passwordHash);
            user.PasswordSaltUser = Convert.ToBase64String(passwordSalt);
            user.IdRole = 3; // По умолчанию роль "User"

            // Используем асинхронные методы
            await Program.context.Users.AddAsync(user);
            await Program.context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> Login(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(u => u.EmailUser == email);

            if (user == null || !VerifyPasswordHash(password, user.PasswordHashUser, user.PasswordSaltUser))
            {
                return null;
            }

            return user;
        }
        public async Task<bool> UserExists(string email)
        {
            return await _context.Users.AnyAsync(u => u.EmailUser == email);
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _context.Users
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(u => u.IdUser == id);
        }
        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
                new(ClaimTypes.Email, user.EmailUser),
                new(ClaimTypes.Role, user.IdRoleNavigation.NameRole)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured")));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public static bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            var salt = Convert.FromBase64String(storedSalt);
            var hash = Convert.FromBase64String(storedHash);

            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Сравниваем байт за байтом
            return computedHash.SequenceEqual(hash);
        }
    }
}
