using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "artist",
                columns: table => new
                {
                    id_artist = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name_artist = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description_artist = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    profile_photo_artist = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    background_photo_artist = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    color_artist = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_artist);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "genre",
                columns: table => new
                {
                    id_genre = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name_genre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    color_genre = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_genre);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "hall",
                columns: table => new
                {
                    id_hall = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name_hall = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description_hall = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    city_hall = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    street_hall = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    building_hall = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_hall);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id_role = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name_role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_role);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "artist_genre",
                columns: table => new
                {
                    id_artist = table.Column<int>(type: "int", nullable: false),
                    id_genre = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.id_artist, x.id_genre })
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "artist_genre_ibfk_1",
                        column: x => x.id_artist,
                        principalTable: "artist",
                        principalColumn: "id_artist",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "artist_genre_ibfk_2",
                        column: x => x.id_genre,
                        principalTable: "genre",
                        principalColumn: "id_genre",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "concert",
                columns: table => new
                {
                    id_concert = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_artist = table.Column<int>(type: "int", nullable: false),
                    id_hall = table.Column<int>(type: "int", nullable: false),
                    age_limit_concert = table.Column<int>(type: "int", nullable: true),
                    status_concert = table.Column<string>(type: "enum('планируется','прошел','отменен')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_start_concert = table.Column<DateOnly>(type: "date", nullable: false),
                    time_start_concert = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_concert);
                    table.ForeignKey(
                        name: "concert_ibfk_1",
                        column: x => x.id_artist,
                        principalTable: "artist",
                        principalColumn: "id_artist",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "concert_ibfk_2",
                        column: x => x.id_hall,
                        principalTable: "hall",
                        principalColumn: "id_hall",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "section",
                columns: table => new
                {
                    id_section = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_hall = table.Column<int>(type: "int", nullable: false),
                    name_section = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    schema_section = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price_section = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_section);
                    table.ForeignKey(
                        name: "section_ibfk_1",
                        column: x => x.id_hall,
                        principalTable: "hall",
                        principalColumn: "id_hall",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id_user = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_role = table.Column<int>(type: "int", nullable: false),
                    first_name_user = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_name_user = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    birth_date_user = table.Column<DateOnly>(type: "date", nullable: false),
                    login_user = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email_user = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    city_user = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash_user = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_user);
                    table.ForeignKey(
                        name: "user_ibfk_1",
                        column: x => x.id_role,
                        principalTable: "role",
                        principalColumn: "id_role",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "favorite",
                columns: table => new
                {
                    id_favorite = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_user = table.Column<int>(type: "int", nullable: false),
                    id_artist = table.Column<int>(type: "int", nullable: false),
                    added_date_favorite = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_favorite);
                    table.ForeignKey(
                        name: "favorite_ibfk_1",
                        column: x => x.id_user,
                        principalTable: "user",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "favorite_ibfk_2",
                        column: x => x.id_artist,
                        principalTable: "artist",
                        principalColumn: "id_artist",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id_notification = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_user = table.Column<int>(type: "int", nullable: false),
                    id_artist = table.Column<int>(type: "int", nullable: false),
                    message_notification = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_notification = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_notification);
                    table.ForeignKey(
                        name: "notification_ibfk_1",
                        column: x => x.id_user,
                        principalTable: "user",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "notification_ibfk_2",
                        column: x => x.id_artist,
                        principalTable: "artist",
                        principalColumn: "id_artist",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "recommendation",
                columns: table => new
                {
                    id_recommendation = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_user = table.Column<int>(type: "int", nullable: false),
                    id_artist = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_recommendation);
                    table.ForeignKey(
                        name: "recommendation_ibfk_1",
                        column: x => x.id_user,
                        principalTable: "user",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "recommendation_ibfk_2",
                        column: x => x.id_artist,
                        principalTable: "artist",
                        principalColumn: "id_artist",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "review",
                columns: table => new
                {
                    id_review = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_user = table.Column<int>(type: "int", nullable: false),
                    id_concert = table.Column<int>(type: "int", nullable: false),
                    text_review = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rating_review = table.Column<int>(type: "int", nullable: true),
                    date_review = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_review);
                    table.ForeignKey(
                        name: "review_ibfk_1",
                        column: x => x.id_user,
                        principalTable: "user",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "review_ibfk_2",
                        column: x => x.id_concert,
                        principalTable: "concert",
                        principalColumn: "id_concert",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "ticket",
                columns: table => new
                {
                    id_ticket = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_concert = table.Column<int>(type: "int", nullable: false),
                    id_user = table.Column<int>(type: "int", nullable: false),
                    id_section = table.Column<int>(type: "int", nullable: false),
                    row_ticket = table.Column<int>(type: "int", nullable: false),
                    seat_ticket = table.Column<int>(type: "int", nullable: false),
                    status_ticket = table.Column<string>(type: "enum('активен','использован','возвращен')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    purchase_date_ticket = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_ticket);
                    table.ForeignKey(
                        name: "ticket_ibfk_1",
                        column: x => x.id_concert,
                        principalTable: "concert",
                        principalColumn: "id_concert",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "ticket_ibfk_2",
                        column: x => x.id_user,
                        principalTable: "user",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "ticket_ibfk_3",
                        column: x => x.id_section,
                        principalTable: "section",
                        principalColumn: "id_section",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "id_genre",
                table: "artist_genre",
                column: "id_genre");

            migrationBuilder.CreateIndex(
                name: "id_artist",
                table: "concert",
                column: "id_artist");

            migrationBuilder.CreateIndex(
                name: "id_hall",
                table: "concert",
                column: "id_hall");

            migrationBuilder.CreateIndex(
                name: "id_artist1",
                table: "favorite",
                column: "id_artist");

            migrationBuilder.CreateIndex(
                name: "id_user",
                table: "favorite",
                column: "id_user");

            migrationBuilder.CreateIndex(
                name: "name_genre",
                table: "genre",
                column: "name_genre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "id_artist2",
                table: "notification",
                column: "id_artist");

            migrationBuilder.CreateIndex(
                name: "id_user1",
                table: "notification",
                column: "id_user");

            migrationBuilder.CreateIndex(
                name: "id_artist3",
                table: "recommendation",
                column: "id_artist");

            migrationBuilder.CreateIndex(
                name: "id_user2",
                table: "recommendation",
                column: "id_user");

            migrationBuilder.CreateIndex(
                name: "id_concert",
                table: "review",
                column: "id_concert");

            migrationBuilder.CreateIndex(
                name: "id_user3",
                table: "review",
                column: "id_user");

            migrationBuilder.CreateIndex(
                name: "name_role",
                table: "role",
                column: "name_role",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "id_hall1",
                table: "section",
                column: "id_hall");

            migrationBuilder.CreateIndex(
                name: "id_concert1",
                table: "ticket",
                column: "id_concert");

            migrationBuilder.CreateIndex(
                name: "id_section",
                table: "ticket",
                column: "id_section");

            migrationBuilder.CreateIndex(
                name: "id_user4",
                table: "ticket",
                column: "id_user");

            migrationBuilder.CreateIndex(
                name: "email_user",
                table: "user",
                column: "email_user",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "id_role",
                table: "user",
                column: "id_role");

            migrationBuilder.CreateIndex(
                name: "login_user",
                table: "user",
                column: "login_user",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "artist_genre");

            migrationBuilder.DropTable(
                name: "favorite");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "recommendation");

            migrationBuilder.DropTable(
                name: "review");

            migrationBuilder.DropTable(
                name: "ticket");

            migrationBuilder.DropTable(
                name: "genre");

            migrationBuilder.DropTable(
                name: "concert");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "section");

            migrationBuilder.DropTable(
                name: "artist");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "hall");
        }
    }
}
