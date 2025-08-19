using System;
using System.Collections.Generic;
using TicketAPI.Models;

namespace TicketAPI.NewModels;

public partial class Artist
{
    public int IdArtist { get; set; }

    public string NameArtist { get; set; } = null!;

    public string? DescriptionArtist { get; set; }

    public string? ProfilePhotoArtist { get; set; }

    public string? BackgroundPhotoArtist { get; set; }

    public string? ColorArtist { get; set; }

    public virtual ICollection<Concert> Concerts { get; set; } = new List<Concert>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

    public virtual ICollection<Genre> IdGenres { get; set; } = new List<Genre>();
}
