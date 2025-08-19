using System;
using System.Collections.Generic;

namespace TicketAPI.Models;

public partial class Genre
{
    public int IdGenre { get; set; }

    public string NameGenre { get; set; } = null!;

    public string ColorGenre { get; set; } = null!;

    public string IconUrlGenre { get; set; } = null!;

    public virtual ICollection<Artist> IdArtists { get; set; } = new List<Artist>();
}
