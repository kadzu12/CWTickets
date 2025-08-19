using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketAPI.Models;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly TicketDbContext _context;
        public NotificationController(
            TicketDbContext context)
        {
            _context = context;
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.IdUser == userId)
                .OrderByDescending(n => n.DateNotification)
                .ToListAsync();

            return Ok(notifications);
        }
        [HttpGet("user/{userId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var count = await _context.Notifications
                .CountAsync(n => n.IdUser == userId && n.IsRead == false);

            return Ok(count);
        }

        [HttpPatch("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.IdNotification == notificationId);

            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(notification);
        }

     
        public static async Task CreateNotification(int userId, string message, int artistId, int concertId)
        {
            var notification = new Notification
            {
                IdUser = userId,
                MessageNotification = message,
                IdArtist = artistId,
                IdConcert = concertId,
                IsRead = false,
                DateNotification = DateTime.UtcNow
            };

            Program.context.Notifications.Add(notification);
            await Program.context.SaveChangesAsync();
        }
    }
}
