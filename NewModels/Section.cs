using System;
using System.Collections.Generic;
using TicketAPI.Models;

namespace TicketAPI.NewModels;

public partial class Section
{
    public int IdSection { get; set; }

    public int IdHall { get; set; }

    public string NameSection { get; set; } = null!;

    public string SchemaSection { get; set; } = null!;

    public int? TotalSeatsSection { get; set; }

    public decimal PriceSection { get; set; }

    public string? TypeSection { get; set; }

    public int? UnitCountSection { get; set; }

    public int? SeatsPerUnitSection { get; set; }

    public virtual Hall IdHallNavigation { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
