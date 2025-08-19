using System;
using System.Collections.Generic;

namespace TicketAPI.Models;

public partial class Hall
{
    public int IdHall { get; set; }

    public string NameHall { get; set; } = null!;

    public string? DescriptionHall { get; set; }

    public string CityHall { get; set; } = null!;

    public string StreetHall { get; set; } = null!;

    public string BuildingHall { get; set; } = null!;

    public virtual ICollection<Concert> Concerts { get; set; } = new List<Concert>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
}
