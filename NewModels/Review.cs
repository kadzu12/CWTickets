using System;
using System.Collections.Generic;
using TicketAPI.Models;

namespace TicketAPI.NewModels;

public partial class Review
{
    public int IdReview { get; set; }

    public int IdUser { get; set; }

    public int IdConcert { get; set; }

    public string? TextReview { get; set; }

    public int RatingReview { get; set; }

    public DateTime DateReview { get; set; }

    public virtual Concert IdConcertNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
