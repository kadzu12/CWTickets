using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace TicketAPI.Models;

public partial class TicketDbContext : DbContext
{
    public TicketDbContext()
    {
    }

    public TicketDbContext(DbContextOptions<TicketDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<Concert> Concerts { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Hall> Halls { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Recommendation> Recommendations { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;user=root;password=qwertyyou123;database=ticket_db", ServerVersion.Parse("8.0.36-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.IdArtist).HasName("PRIMARY");

            entity.ToTable("artist");

            entity.Property(e => e.IdArtist).HasColumnName("id_artist");
            entity.Property(e => e.BackgroundPhotoArtist)
                .HasMaxLength(255)
                .HasColumnName("background_photo_artist");
            entity.Property(e => e.ColorArtist)
                .HasMaxLength(20)
                .HasColumnName("color_artist");
            entity.Property(e => e.DescriptionArtist)
                .HasColumnType("text")
                .HasColumnName("description_artist");
            entity.Property(e => e.NameArtist)
                .HasMaxLength(100)
                .HasColumnName("name_artist");
            entity.Property(e => e.ProfilePhotoArtist)
                .HasMaxLength(255)
                .HasColumnName("profile_photo_artist");

            entity.HasMany(d => d.IdGenres).WithMany(p => p.IdArtists)
                .UsingEntity<Dictionary<string, object>>(
                    "ArtistGenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("IdGenre")
                        .HasConstraintName("artist_genre_ibfk_2"),
                    l => l.HasOne<Artist>().WithMany()
                        .HasForeignKey("IdArtist")
                        .HasConstraintName("artist_genre_ibfk_1"),
                    j =>
                    {
                        j.HasKey("IdArtist", "IdGenre")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("artist_genre");
                        j.HasIndex(new[] { "IdGenre" }, "id_genre");
                        j.IndexerProperty<int>("IdArtist").HasColumnName("id_artist");
                        j.IndexerProperty<int>("IdGenre").HasColumnName("id_genre");
                    });
        });

        modelBuilder.Entity<Concert>(entity =>
        {
            entity.HasKey(e => e.IdConcert).HasName("PRIMARY");

            entity.ToTable("concert");

            entity.HasIndex(e => e.IdArtist, "id_artist");

            entity.HasIndex(e => e.IdHall, "id_hall");

            entity.Property(e => e.IdConcert).HasColumnName("id_concert");
            entity.Property(e => e.AgeLimitConcert).HasColumnName("age_limit_concert");
            entity.Property(e => e.DateStartConcert).HasColumnName("date_start_concert");
            entity.Property(e => e.IdArtist).HasColumnName("id_artist");
            entity.Property(e => e.IdHall).HasColumnName("id_hall");
            entity.Property(e => e.StatusConcert)
                .HasColumnType("enum('Событие планируется','Событие прошло','Событие отменено')")
                .HasColumnName("status_concert");
            entity.Property(e => e.TimeStartConcert)
                .HasColumnType("time")
                .HasColumnName("time_start_concert");

            entity.HasOne(d => d.IdArtistNavigation).WithMany(p => p.Concerts)
                .HasForeignKey(d => d.IdArtist)
                .HasConstraintName("concert_ibfk_1");

            entity.HasOne(d => d.IdHallNavigation).WithMany(p => p.Concerts)
                .HasForeignKey(d => d.IdHall)
                .HasConstraintName("concert_ibfk_2");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.IdFavorite).HasName("PRIMARY");

            entity.ToTable("favorite");

            entity.HasIndex(e => e.IdArtist, "id_artist");

            entity.HasIndex(e => e.IdUser, "id_user");

            entity.Property(e => e.IdFavorite).HasColumnName("id_favorite");
            entity.Property(e => e.AddedDateFavorite)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("added_date_favorite");
            entity.Property(e => e.IdArtist).HasColumnName("id_artist");
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasOne(d => d.IdArtistNavigation).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.IdArtist)
                .HasConstraintName("favorite_ibfk_2");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("favorite_ibfk_1");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.IdGenre).HasName("PRIMARY");

            entity.ToTable("genre");

            entity.HasIndex(e => e.NameGenre, "name_genre").IsUnique();

            entity.Property(e => e.IdGenre).HasColumnName("id_genre");
            entity.Property(e => e.ColorGenre)
                .HasMaxLength(20)
                .HasColumnName("color_genre");
            entity.Property(e => e.IconUrlGenre)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("icon_url_genre");
            entity.Property(e => e.NameGenre)
                .HasMaxLength(100)
                .HasColumnName("name_genre");
        });

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.HasKey(e => e.IdHall).HasName("PRIMARY");

            entity.ToTable("hall");

            entity.Property(e => e.IdHall).HasColumnName("id_hall");
            entity.Property(e => e.BuildingHall)
                .HasMaxLength(10)
                .HasColumnName("building_hall");
            entity.Property(e => e.CityHall)
                .HasMaxLength(50)
                .HasColumnName("city_hall");
            entity.Property(e => e.DescriptionHall)
                .HasColumnType("text")
                .HasColumnName("description_hall");
            entity.Property(e => e.NameHall)
                .HasMaxLength(100)
                .HasColumnName("name_hall");
            entity.Property(e => e.StreetHall)
                .HasMaxLength(100)
                .HasColumnName("street_hall");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.IdNotification).HasName("PRIMARY");

            entity.ToTable("notification");

            entity.HasIndex(e => e.IdArtist, "id_artist");

            entity.HasIndex(e => e.IdConcert, "id_concert");

            entity.HasIndex(e => e.IdUser, "id_user");

            entity.Property(e => e.IdNotification).HasColumnName("id_notification");
            entity.Property(e => e.DateNotification)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("date_notification");
            entity.Property(e => e.IdArtist).HasColumnName("id_artist");
            entity.Property(e => e.IdConcert).HasColumnName("id_concert");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IsRead)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_read");
            entity.Property(e => e.MessageNotification)
                .HasColumnType("text")
                .HasColumnName("message_notification");

            entity.HasOne(d => d.IdArtistNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.IdArtist)
                .HasConstraintName("notification_ibfk_2");

            entity.HasOne(d => d.IdConcertNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.IdConcert)
                .HasConstraintName("notification_ibfk_3");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("notification_ibfk_1");
        });

        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.HasKey(e => e.IdRecommendation).HasName("PRIMARY");

            entity.ToTable("recommendation");

            entity.HasIndex(e => e.IdArtist, "id_artist");

            entity.HasIndex(e => e.IdUser, "id_user");

            entity.Property(e => e.IdRecommendation).HasColumnName("id_recommendation");
            entity.Property(e => e.IdArtist).HasColumnName("id_artist");
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasOne(d => d.IdArtistNavigation).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.IdArtist)
                .HasConstraintName("recommendation_ibfk_2");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("recommendation_ibfk_1");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.IdReview).HasName("PRIMARY");

            entity.ToTable("review");

            entity.HasIndex(e => e.IdConcert, "id_concert");

            entity.HasIndex(e => e.IdUser, "id_user");

            entity.Property(e => e.IdReview).HasColumnName("id_review");
            entity.Property(e => e.DateReview)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("date_review");
            entity.Property(e => e.IdConcert).HasColumnName("id_concert");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.RatingReview).HasColumnName("rating_review");
            entity.Property(e => e.TextReview)
                .HasColumnType("text")
                .HasColumnName("text_review");

            entity.HasOne(d => d.IdConcertNavigation).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.IdConcert)
                .HasConstraintName("review_ibfk_2");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("review_ibfk_1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("PRIMARY");

            entity.ToTable("role");

            entity.HasIndex(e => e.NameRole, "name_role").IsUnique();

            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.NameRole)
                .HasMaxLength(50)
                .HasColumnName("name_role");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.IdSection).HasName("PRIMARY");

            entity.ToTable("section");

            entity.HasIndex(e => e.IdHall, "id_hall");

            entity.Property(e => e.IdSection).HasColumnName("id_section");
            entity.Property(e => e.IdHall).HasColumnName("id_hall");
            entity.Property(e => e.NameSection)
                .HasMaxLength(100)
                .HasColumnName("name_section");
            entity.Property(e => e.PriceSection)
                .HasPrecision(10, 2)
                .HasColumnName("price_section");
            entity.Property(e => e.SchemaSection)
                .HasColumnType("text")
                .HasColumnName("schema_section");
            entity.Property(e => e.SeatsPerUnitSection).HasColumnName("seats_per_unit_section");
            entity.Property(e => e.TotalSeatsSection).HasColumnName("total_seats_section");
            entity.Property(e => e.TypeSection)
                .HasColumnType("enum('Танцпол','Ряды','Столы')")
                .HasColumnName("type_section");
            entity.Property(e => e.UnitCountSection).HasColumnName("unit_count_section");

            entity.HasOne(d => d.IdHallNavigation).WithMany(p => p.Sections)
                .HasForeignKey(d => d.IdHall)
                .HasConstraintName("section_ibfk_1");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.IdTicket).HasName("PRIMARY");

            entity.ToTable("ticket");

            entity.HasIndex(e => e.IdConcert, "id_concert");

            entity.HasIndex(e => e.IdSection, "id_section");

            entity.HasIndex(e => e.IdUser, "id_user");

            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.CardNumberTicket)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("card_number_ticket");
            entity.Property(e => e.IdConcert).HasColumnName("id_concert");
            entity.Property(e => e.IdSection).HasColumnName("id_section");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.PurchaseDateTicket)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("purchase_date_ticket");
            entity.Property(e => e.RowTicket).HasColumnName("row_ticket");
            entity.Property(e => e.SeatTicket).HasColumnName("seat_ticket");
            entity.Property(e => e.StatusTicket)
                .HasColumnType("enum('Активен','Использован','Возвращен')")
                .HasColumnName("status_ticket");

            entity.HasOne(d => d.IdConcertNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdConcert)
                .HasConstraintName("ticket_ibfk_1");

            entity.HasOne(d => d.IdSectionNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdSection)
                .HasConstraintName("ticket_ibfk_3");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("ticket_ibfk_2");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PRIMARY");

            entity.ToTable("user");

            entity.HasIndex(e => e.EmailUser, "email_user").IsUnique();

            entity.HasIndex(e => e.IdRole, "id_role");

            entity.HasIndex(e => e.LoginUser, "login_user").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.BirthDateUser).HasColumnName("birth_date_user");
            entity.Property(e => e.CityUser)
                .HasMaxLength(50)
                .HasColumnName("city_user");
            entity.Property(e => e.EmailUser)
                .HasMaxLength(100)
                .HasColumnName("email_user");
            entity.Property(e => e.FirstNameUser)
                .HasMaxLength(50)
                .HasColumnName("first_name_user");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.LastNameUser)
                .HasMaxLength(50)
                .HasColumnName("last_name_user");
            entity.Property(e => e.LoginUser)
                .HasMaxLength(50)
                .HasColumnName("login_user");
            entity.Property(e => e.PasswordHashUser)
                .HasColumnType("text")
                .HasColumnName("password_hash_user");
            entity.Property(e => e.PasswordSaltUser)
                .HasColumnType("text")
                .HasColumnName("password_salt_user");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRole)
                .HasConstraintName("user_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
