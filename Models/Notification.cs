using System;
using System.Collections.Generic;

namespace TicketAPI.Models;

public partial class Notification
{
    public int IdNotification { get; set; }

    public int IdUser { get; set; }

    public int IdArtist { get; set; }

    public int IdConcert { get; set; }

    public string MessageNotification { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? DateNotification { get; set; }

    public virtual Artist IdArtistNavigation { get; set; } = null!;

    public virtual Concert IdConcertNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
