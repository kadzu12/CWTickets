using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TicketAPI.Models;

public partial class User
{
    public int IdUser { get; set; }

    public int IdRole { get; set; }

    public string FirstNameUser { get; set; } = null!;

    public string LastNameUser { get; set; } = null!;

    public DateOnly BirthDateUser { get; set; }

    public string LoginUser { get; set; } = null!;

    public string EmailUser { get; set; } = null!;

    public string? CityUser { get; set; }
    [MaxLength(200)]

    public string PasswordHashUser { get; set; } = null!;
    [MaxLength(200)]
    public string PasswordSaltUser { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual Role IdRoleNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    [JsonIgnore]
    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    [JsonIgnore]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    [JsonIgnore]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
