using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicketAPI.Middleware;
using TicketAPI.Models;
using TicketAPI.Services;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using DinkToPdf.Contracts;
using DinkToPdf;

internal class Program
{
    public static TicketDbContext context { get; } = new TicketDbContext();
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // В разделе сервисов добавьте
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
        //builder.Logging.ClearProviders();
        //builder.Logging.AddConsole();
        //builder.Services.AddRazorPages();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowVueApp",
                policy => policy.WithOrigins("http://localhost:5173")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
        });
        //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //    .AddJwtBearer(options =>
        //    {
        //        options.TokenValidationParameters = new TokenValidationParameters
        //        {
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
        //                .GetBytes(builder.Configuration["Jwt:Key"])),
        //            ValidateIssuer = false,
        //            ValidateAudience = false,
        //            ValidateLifetime = true,
        //            ClockSkew = TimeSpan.Zero
        //        };
        //    });

        //builder.Services.AddAuthorization(options =>
        //{
        //    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Администратор"));
        //    options.AddPolicy("ModeratorOnly", policy => policy.RequireRole("Модератор"));
        //    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
        //});
        // Add services to the container.
        builder.Services.AddDbContext<TicketDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 32)) // укажи свою версию MySQL
    )
);
        builder.Services.AddHostedService<NotificationSchedulerService>();
        builder.Services.AddHostedService<RecommendationSchedulerService>();
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //if (!app.Environment.IsDevelopment())
        //{
        //    app.UseExceptionHandler("/Error");
        //    app.UseHsts();
        //}

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors("AllowVueApp");
        //app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}