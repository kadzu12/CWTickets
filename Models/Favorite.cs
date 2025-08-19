using System;
using System.Collections.Generic;

namespace TicketAPI.Models;

public partial class Favorite
{
    public int IdFavorite { get; set; }

    public int IdUser { get; set; }

    public int IdArtist { get; set; }

    public DateTime? AddedDateFavorite { get; set; }

    public virtual Artist IdArtistNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
