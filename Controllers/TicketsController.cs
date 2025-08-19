using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.ComponentModel.DataAnnotations;
using TicketAPI.Models;
using static TicketAPI.Controllers.TicketsController;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly TicketDbContext _context;
        private readonly IConverter _converter;
        private readonly string _baseUrl;

        [HttpGet("qr/{ticketId}")]
        public IActionResult GetTicketQrCode(int ticketId)
        {
            try
            {
                // Получаем полную информацию о билете из базы данных
                var ticket = _context.Tickets
                    .Include(t => t.IdConcertNavigation)
                        .ThenInclude(c => c.IdArtistNavigation)
                    .Include(t => t.IdConcertNavigation)
                        .ThenInclude(c => c.IdHallNavigation)
                    .Include(t => t.IdSectionNavigation)
                    .FirstOrDefault(t => t.IdTicket == ticketId);

                if (ticket == null)
                {
                    return NotFound("Билет не найден");
                }

                var qrContent = $"Билет №: {ticket.IdTicket}\n" +
                                $"========================\n" +
                                $"Артист: {ticket.IdConcertNavigation?.IdArtistNavigation?.NameArtist ?? "Не указан"}\n" +
                                $"Дата: {ticket.IdConcertNavigation?.DateStartConcert:dd.MM.yyyy}\n" +
                                $"Время: {ticket.IdConcertNavigation?.TimeStartConcert:HH\\:mm}\n" +
                                $"Зал: {ticket.IdConcertNavigation?.IdHallNavigation?.NameHall ?? "Не указан"}\n" +
                                $"Город: {ticket.IdConcertNavigation?.IdHallNavigation?.CityHall ?? "Не указан"}\n" +
                                $"Сектор: {ticket.IdSectionNavigation?.NameSection ?? "Не указан"}\n" +
                                $"Ряд: {ticket.RowTicket}\n" +
                                $"Место: {ticket.SeatTicket}\n" +
                                $"Статус: {ticket.StatusTicket}\n" +
                                $"Цена: {ticket.IdSectionNavigation.PriceSection} руб.\n" +
                                $"Дата покупки: {ticket.PurchaseDateTicket:dd.MM.yyyy HH:mm}\n" +
                                $"========================\n" +
                                $"CWTICKETS";

                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);

                return File(qrCodeImage, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Произошла ошибка при генерации QR-кода");
            }
        }

        public TicketsController(
            TicketDbContext context,
            IConverter converter,
            IConfiguration configuration)
        {
            _context = context;
            _converter = converter;
            _baseUrl = configuration["AppSettings:BaseUrl"];
        }
        [HttpPost("Purchase")]
        public async Task<IActionResult> PurchaseTickets([FromBody] PurchaseTicketsDto purchaseDto)
        {
            try
            {
                // Валидация входных данных
                if (purchaseDto == null || purchaseDto.Tickets == null || !purchaseDto.Tickets.Any())
                {
                    return BadRequest("Необходимо указать хотя бы один билет");
                }

                var firstTicket = purchaseDto.Tickets.First();

                // Проверка существования концерта
                var concert = await _context.Concerts
                    .Include(c => c.IdArtistNavigation)
                    .Include(c => c.IdHallNavigation)
                    .FirstOrDefaultAsync(c => c.IdConcert == firstTicket.IdConcert);

                if (concert == null)
                {
                    return NotFound("Концерт не найден");
                }

                var userTicketsCount = await _context.Tickets
                    .CountAsync(t => t.IdUser == firstTicket.IdUser && t.StatusTicket == "Активен");

                if (userTicketsCount + purchaseDto.Tickets.Count > 5)
                {
                    return BadRequest($"Нельзя приобрести более 5 билетов. У вас уже {userTicketsCount} активных билетов."
                    );
                }

                var ticketsBySection = purchaseDto.Tickets
                    .GroupBy(t => t.IdSection)
                    .ToList();

                var tickets = new List<Ticket>();
                var now = DateTime.Now;

                foreach (var sectionGroup in ticketsBySection)
                {
                    var section = await _context.Sections
                        .FirstOrDefaultAsync(s => s.IdSection == sectionGroup.Key);

                    if (section == null)
                    {
                        return BadRequest($"Секция с ID {sectionGroup.Key} не найдена");
                    }

                    if (section.TypeSection != "Танцпол")
                    {
                        foreach (var ticketDto in sectionGroup)
                        {
                            if (ticketDto.RowTicket == null || ticketDto.SeatTicket == null)
                            {
                                continue;
                            }

                            var isTaken = await _context.Tickets
                                .AnyAsync(t => t.IdConcert == ticketDto.IdConcert &&
                                             t.IdSection == ticketDto.IdSection &&
                                             t.RowTicket == ticketDto.RowTicket &&
                                             t.SeatTicket == ticketDto.SeatTicket &&
                                             t.StatusTicket == "Активен");

                            if (isTaken)
                            {
                                string errorMessage = section.TypeSection switch
                                {
                                    "Столы" => $"Место {ticketDto.SeatTicket} за столом {ticketDto.RowTicket} уже занято",
                                    "Ряды" => $"Место {ticketDto.SeatTicket} в ряду {ticketDto.RowTicket} уже занято",
                                    _ => "Выбранное место уже занято"
                                };

                                return BadRequest(new
                                {
                                    Success = false,
                                    Message = errorMessage
                                });
                            }
                        }
                    }

                    foreach (var ticketDto in sectionGroup)
                    {
                        var ticket = new Ticket
                        {
                            IdConcert = ticketDto.IdConcert,
                            IdUser = ticketDto.IdUser,
                            IdSection = ticketDto.IdSection,
                            RowTicket = ticketDto.RowTicket,
                            SeatTicket = ticketDto.SeatTicket,
                            StatusTicket = "Активен",
                            PurchaseDateTicket = now,
                            CardNumberTicket = purchaseDto.CardNumber
                        };

                        tickets.Add(ticket);
                    }
                }

                await _context.Tickets.AddRangeAsync(tickets);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Билеты успешно приобретены",
                    PurchasedTickets = tickets.Count,
                    TotalUserTickets = userTicketsCount + tickets.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        public class PurchaseTicketsDto
        {
            public List<TicketPurchaseDto> Tickets { get; set; }
            public string CardNumber { get; set; }

        }

        public class TicketPurchaseDto
        {
            public int IdConcert { get; set; }

            public int IdUser { get; set; }
            public int IdSection { get; set; }

            public int? RowTicket { get; set; }

            public int? SeatTicket { get; set; }

        }
        [HttpGet("GetOccupiedSeats/{concertId}")]
        public async Task<IActionResult> GetOccupiedSeats(int concertId)
        {
            var tickets = await _context.Tickets
                .Where(t => t.IdConcert == concertId && t.StatusTicket == "Активен")
                .Select(t => new
                {
                    SectionId = t.IdSection,
                    Row = t.RowTicket,
                    Seat = t.SeatTicket
                })
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpGet("CheckSeatAvailability")]
        public async Task<IActionResult> CheckSeatAvailability(
    [FromQuery] int concertId,
    [FromQuery] int sectionId,
    [FromQuery] int? rowNumber,
    [FromQuery] int? seatNumber)
        {
            var section = await _context.Sections.FindAsync(sectionId);
            if (section == null)
            {
                return NotFound(new { IsAvailable = false });
            }

            if (section.TypeSection == "Танцпол")
            {
                var soldTicketsCount = await _context.Tickets
                    .CountAsync(t => t.IdConcert == concertId &&
                                   t.IdSection == sectionId);

                return Ok(new
                {
                    IsAvailable = soldTicketsCount < section.TotalSeatsSection
                });
            }
            else
            {
                if (rowNumber == null || seatNumber == null)
                {
                    return BadRequest(new { IsAvailable = false });
                }

                var isTaken = await _context.Tickets
                    .AnyAsync(t => t.IdConcert == concertId &&
                                 t.IdSection == sectionId &&
                                 t.RowTicket == rowNumber &&
                                 t.SeatTicket == seatNumber &&
                                 t.StatusTicket == "Активен");

                return Ok(new { IsAvailable = !isTaken });
            }
        }

        //private async Task SendConfirmationEmails(List<Ticket> tickets, string email)
        //{
        //    try
        //    {
        //        var concert = await _context.Concerts
        //            .Include(c => c.IdArtistNavigation)
        //            .Include(c => c.IdHallNavigation)
        //            .FirstOrDefaultAsync(c => c.IdConcert == tickets.First().IdConcert);

        //        var sections = await _context.Sections
        //            .Where(s => tickets.Select(t => t.IdSection).Contains(s.IdSection))
        //            .ToDictionaryAsync(s => s.IdSection);

        //        foreach (var ticket in tickets)
        //        {
        //            ticket.IdConcertNavigation = concert;
        //            ticket.IdSectionNavigation = sections[ticket.IdSection];

        //            // Здесь реализация отправки email
        //            // await _emailService.SendTicketEmail(email, ticket);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Ошибка при отправке email подтверждения");
        //    }
        //}
        [HttpGet("download/{ticketId}/{userId}")]
        public async Task<IActionResult> DownloadTicketPdf(int ticketId, int userId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.IdConcertNavigation)
                .ThenInclude(c => c.IdArtistNavigation)
                .Include(t => t.IdConcertNavigation.IdHallNavigation)
                .Include(t => t.IdSectionNavigation)
                .FirstOrDefaultAsync(t => t.IdTicket == ticketId && t.IdUser == userId);

            if (ticket == null)
            {
                return NotFound("Билет не найден или не принадлежит пользователю");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.IdUser == userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            var htmlContent = GenerateTicketHtml(ticket, user);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4,
        },
                Objects = {
            new ObjectSettings() {
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" },
            }
        }
            };

            var pdfBytes = _converter.Convert(doc);

            return File(pdfBytes, "application/pdf", $"Билет_{ticket.IdTicket}.pdf");
        }

        private string GenerateTicketHtml(Ticket ticket, User user)
        {
            var projectRoot = Directory.GetCurrentDirectory();
            var imagePath = Path.Combine(projectRoot, "AppAssets", "new_back_ticket.png");
            var imageUrl = $"file:///{imagePath.Replace("\\", "/")}";

            var qrUrl = $"http://localhost:5199/api/Tickets/qr/{ticket.IdTicket}";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 0;
            background-color: white;
            background-size: cover;
            background-position: center;
        }}
        .ticket {{
            max-width: 600px;
            margin: 20px auto;
            padding: 30px;
            position: relative;
        }}
        .border-ticket-info {{
            display: flex;
            flex-direction: row;
            border: 2px solid #000;
            padding: 20px;
        }}
        .event-header {{
            text-align: start;
            margin-bottom: 15px;
        }}
        .event-title {{
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 5px;
        }}
        .event-date {{
            font-size: 18px;
            margin-bottom: 5px;
        }}
        .event-venue {{
            font-size: 16px;
            font-weight: bold;
        }}
        .event-venue-info {{
            font-size: 16px;
            margin-bottom: 15px;
        }}
        .ticket-section {{
            margin-bottom: 15px;
        }}
        .section-title {{
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 10px;
        }}
        .section-details {{
            margin-bottom: 5px;
        }}
        .holder-name {{
            font-size: 18px;
            font-weight: bold;
            text-align: start;
            margin: 10px 0;
        }}
        .border-qr-info {{
            margin-left:63%;
            margin-top:-38%;
            align-items: end;
            justify-items: end;
            border: 2px solid #000;
        }}
        .qr-code * {{
            height: 200px;
            width: 200px;
        }}
        .banner {{
            display: flex;
            justify-content: space-between;
            margin: 0;
            padding: 10px 10%;
            overflow: hidden;
        }}
        .banner-content {{
            align-items: start;
            max-width: 500px;
            text-align: left;
        }}
        .banner-content h1 {{
            z-index: 10;
            margin-left: -55px;
            font-size: 25px;
            font-weight: 700;
            color: #222;
            width: 280px;
        }}
        .banner-content span {{
            z-index: 10;
            color: #ff5757;
            font-size: 35px;
        }}
        .banner-content p {{
            width: 260px;
            z-index: 10;
            margin-left: -55px;
            font-size: 20px;
            color: #555;
            margin-top: 15px;
        }}
        .ticket-image img {{
            margin-top: -13rem;
            margin-left:16%;
            position: absolute;
            width: auto;
            height: 45%;
            z-index: -10;
            overflow: hidden;
        }}
        .ticketmaster-footer {{
            text-align: start;
            font-weight: bold;
            margin-top: 20px;
            margin-left: -20px;
        }}
    </style>
</head>
<body>
    <div class='ticket'>
        <div class='border-ticket-info'>
            <div class='ticket-conteiner'>
                <div class='event-header'>
                    <div class='event-title'>{ticket.IdConcertNavigation.IdArtistNavigation.NameArtist}</div>
                    <div class='event-date'>{ticket.IdConcertNavigation.DateStartConcert:dd MMMM yyyy} - {ticket.IdConcertNavigation.TimeStartConcert:HH:mm}</div>
                    <div class='event-venue'>{ticket.IdConcertNavigation.IdHallNavigation.NameHall}, {ticket.IdConcertNavigation.IdHallNavigation.CityHall}</div>
                        <div class='event-venue-info'>ул. {ticket.IdConcertNavigation.IdHallNavigation.StreetHall}, {ticket.IdConcertNavigation.IdHallNavigation.BuildingHall}</div>
                </div>
                <div class='ticket-section'>
                    <div class='section-title'>Секция - {ticket.IdSectionNavigation.NameSection}</div>
                    <div class='section-details'><strong>Ряд:</strong> {ticket.RowTicket} - <strong>Место:</strong> {ticket.SeatTicket}</div>
                </div>
                <div class='holder-name'>
                    {user.FirstNameUser} {user.LastNameUser}
                </div>
            </div>
            <div class='qr-code'>
                <div class='border-qr-info'>
                    <img src='{qrUrl}' alt='qr'>
                </div>
            </div>
        </div>
        <div class='banner'>
            <div class='banner-content'>
                <h1>Погружайтесь в волну <span>вместе с нами!</span></h1>
                <p>Открывай новые горизонты и наслаждайся живой музыкой.</p>
                <div class='ticketmaster-footer'>
                    CWTICKETS
                </div>
            </div>
            <div class='ticket-image'>
                <img src='{imageUrl}' alt='Tickets' />
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}
