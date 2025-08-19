using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using TicketAPI.Models;

namespace TicketAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _fromAddress;
        private readonly string _baseUrl;

        public EmailService(IConfiguration configuration)
        {
            _fromAddress = configuration["EmailSettings:FromAddress"];
            _baseUrl = configuration["AppSettings:BaseUrl"];

            _smtpClient = new SmtpClient(configuration["EmailSettings:SmtpHost"])
            {
                Port = int.Parse(configuration["EmailSettings:SmtpPort"]),
                Credentials = new NetworkCredential(
                    configuration["EmailSettings:Username"],
                    configuration["EmailSettings:Password"]),
                EnableSsl = bool.Parse(configuration["EmailSettings:UseSsl"]),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };
        }

        public async Task SendTicketEmailAsync(string email, Ticket ticket)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromAddress),
                Subject = $"Ваш билет на концерт {ticket.IdConcertNavigation.IdArtistNavigation.NameArtist}",
                Body = GenerateTicketEmailBody(ticket),
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await _smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromAddress),
                Subject = "Добро пожаловать на наш сервис!",
                Body = GenerateWelcomeEmailBody(userName),
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await _smtpClient.SendMailAsync(mailMessage);
        }

        private string GenerateTicketEmailBody(Ticket ticket)
        {
            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                .ticket-container {{ max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; }}
                .header {{ background-color: #ff5757; color: white; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; }}
                .ticket-info {{ margin-bottom: 20px; }}
                .qr-code {{ text-align: center; margin: 20px 0; }}
                .footer {{ background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #777; }}
                .info-label {{ color: #777; }}
                .info-value {{ font-weight: bold; margin-bottom: 10px; }}
            </style>
        </head>
        <body>
            <div class='ticket-container'>
                <div class='header'>
                    <h1>Электронный билет</h1>
                    <p>№ {ticket.IdTicket}</p>
                </div>
                
                <div class='content'>
                    <div class='ticket-info'>
                        <h2>{ticket.IdConcertNavigation.IdArtistNavigation.NameArtist}</h2>
                        <p class='info-label'>Дата и время</p>
                        <p class='info-value'>{ticket.IdConcertNavigation.DateStartConcert:dd.MM.yyyy} в {ticket.IdConcertNavigation.TimeStartConcert:h\\:mm}</p>
                        
                        <p class='info-label'>Место проведения</p>
                        <p class='info-value'>{ticket.IdConcertNavigation.IdHallNavigation.NameHall}, {ticket.IdConcertNavigation.IdHallNavigation.CityHall}</p>
                        
                        <p class='info-label'>Секция</p>
                        <p class='info-value'>{ticket.IdSectionNavigation.NameSection} (Ряд {ticket.RowTicket}, Место {ticket.SeatTicket})</p>
                        
                        <p class='info-label'>Цена</p>
                        <p class='info-value'>{ticket.IdSectionNavigation.PriceSection} ₽</p>
                    </div>
                    
                    <div class='qr-code'>
                        <img src='{_baseUrl}/api/tickets/qr/{ticket.IdTicket}' alt='QR-код билета' width='200'>
                        <p>Отсканируйте QR-код при входе</p>
                    </div>
                </div>
                
                <div class='footer'>
                    <p>Это письмо сформировано автоматически. Пожалуйста, не отвечайте на него.</p>
                    <p>© {DateTime.Now.Year} Concert Ticket Service</p>
                </div>
            </div>
        </body>
        </html>";
        }

        private string GenerateWelcomeEmailBody(string userName)
        {
            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                .container {{ max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; }}
                .header {{ background-color: #ff5757; color: white; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; }}
                .footer {{ background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #777; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>Добро пожаловать!</h1>
                </div>
                
                <div class='content'>
                    <p>Уважаемый(ая) {userName},</p>
                    <p>Благодарим вас за регистрацию в нашем сервисе продажи билетов на концерты.</p>
                    <p>Теперь вы можете:</p>
                    <ul>
                        <li>Покупать билеты онлайн</li>
                        <li>Просматривать историю покупок</li>
                        <li>Получать электронные билеты на email</li>
                    </ul>
                    <p>Приятного пользования!</p>
                </div>
                
                <div class='footer'>
                    <p>Это письмо сформировано автоматически. Пожалуйста, не отвечайте на него.</p>
                    <p>© {DateTime.Now.Year} Concert Ticket Service</p>
                </div>
            </div>
        </body>
        </html>";
        }
    }
}
