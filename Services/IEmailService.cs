using TicketAPI.Models;

namespace TicketAPI.Services
{
    public interface IEmailService
    {
        Task SendTicketEmailAsync(string email, Ticket ticket);
        Task SendWelcomeEmailAsync(string email, string userName);
    }
}
