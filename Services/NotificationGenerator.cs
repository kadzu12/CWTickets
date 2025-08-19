using Microsoft.EntityFrameworkCore;
using TicketAPI.Controllers;
using TicketAPI.Models;

namespace TicketAPI.Services
{
    public class NotificationGenerator
    {
        private readonly TicketDbContext _context;

        public NotificationGenerator(TicketDbContext context)
        {
            _context = context;
        }

        public async Task GenerateReminderAndFeedbackNotifications()
        {
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);
            var today = DateTime.UtcNow.Date;

            var tomorrowConcerts = await _context.Concerts
                .Include(c => c.Tickets)
                .Include(c => c.IdArtistNavigation)
                .Include(c=>c.IdHallNavigation)
                .Where(c => c.DateStartConcert == DateOnly.FromDateTime(tomorrow))
                .ToListAsync();

            foreach (var concert in tomorrowConcerts)
            {
                var userIds = concert.Tickets.Select(t => t.IdUser).Distinct();
                foreach (var userId in userIds)
                {
                    await NotificationController.CreateNotification(userId,
                        $"Напоминаем: завтра концерт {concert.IdArtistNavigation.NameArtist} в {concert.TimeStartConcert} на концертной площадке {concert.IdHallNavigation.NameHall}",
                        concert.IdArtist, concert.IdConcert);
                }
            }

            var pastConcerts = await _context.Concerts
                .Where(c => c.DateStartConcert < DateOnly.FromDateTime(today))
                .Include(c=>c.IdHallNavigation)
                .Include(c=>c.IdArtistNavigation)
                .Include(c => c.Tickets)
                .ToListAsync();

            foreach (var concert in pastConcerts)
            {
                var userIds = concert.Tickets.Select(t => t.IdUser).Distinct();
                foreach (var userId in userIds)
                {
                    var hasReview = await _context.Reviews
                        .AnyAsync(r => r.IdUser == userId && r.IdConcert == concert.IdConcert);
                    if (!hasReview)
                    {
                        await NotificationController.CreateNotification(userId,
                            $"Концерт {concert.IdArtistNavigation.NameArtist} на концертной площадке {concert.IdHallNavigation.NameHall} уже прошёл. Оставьте ваш отзыв!",
                            concert.IdArtist, concert.IdConcert);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }

}
