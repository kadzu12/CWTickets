using System;
using System.Collections.Generic;

namespace TicketAPI.Models;

public partial class Review
{
    public int IdReview { get; set; }

    public int IdUser { get; set; }

    public int IdConcert { get; set; }

    public string TextReview { get; set; } = null!;

    public int? RatingReview { get; set; }

    public DateTime? DateReview { get; set; }

    public virtual Concert IdConcertNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
