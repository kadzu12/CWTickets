using System;
using System.Collections.Generic;

namespace TicketAPI.Models;

public partial class Ticket
{
    public int IdTicket { get; set; }

    public int IdConcert { get; set; }

    public int IdUser { get; set; }

    public int IdSection { get; set; }

    public int? RowTicket { get; set; }

    public int? SeatTicket { get; set; }

    public string StatusTicket { get; set; } = null!;

    public DateTime? PurchaseDateTicket { get; set; }

    public string CardNumberTicket { get; set; } = null!;

    public virtual Concert IdConcertNavigation { get; set; } = null!;

    public virtual Section IdSectionNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
