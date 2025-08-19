using System;
using System.Collections.Generic;

namespace TicketAPI.Models;

public partial class Concert
{
    public int IdConcert { get; set; }

    public int IdArtist { get; set; }

    public int IdHall { get; set; }

    public int? AgeLimitConcert { get; set; }

    public string StatusConcert { get; set; } = null!;

    public DateOnly DateStartConcert { get; set; }

    public TimeOnly TimeStartConcert { get; set; }

    public virtual Artist IdArtistNavigation { get; set; } = null!;

    public virtual Hall IdHallNavigation { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
