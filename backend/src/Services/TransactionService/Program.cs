using EasyTrack.TransactionService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// DbContext Registration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"] 
    ?? builder.Configuration["ConnectionStrings__DefaultConnection"];

builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register JWT Bearer Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var secret = builder.Configuration["Jwt:Secret"] ?? builder.Configuration["Jwt__Secret"] ?? "your_super_secret_jwt_key_with_at_least_32_characters_123456";
    var issuer = builder.Configuration["Jwt:Issuer"] ?? builder.Configuration["Jwt__Issuer"] ?? "EasyTrack";
    var audience = builder.Configuration["Jwt:Audience"] ?? builder.Configuration["Jwt__Audience"] ?? "EasyTrackClient";

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ClockSkew = TimeSpan.Zero
    };
});

// Register CORS
var allowedOriginsSetting = builder.Configuration["Cors:AllowedOrigins"] ?? builder.Configuration["Cors__AllowedOrigins"];
var allowedOrigins = !string.IsNullOrEmpty(allowedOriginsSetting)
    ? allowedOriginsSetting.Split(',', StringSplitOptions.RemoveEmptyEntries)
    : new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Automatically apply migrations with failure tolerance
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<TransactionDbContext>();
        logger.LogInformation("Applying migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrated successfully.");

        // Custom runtime seeding for new default categories (Rent, Water bill, Electricity bill, Other)
        var newCategories = new System.Collections.Generic.List<EasyTrack.TransactionService.Core.Entities.Category>
        {
            new EasyTrack.TransactionService.Core.Entities.Category { Id = new Guid("99999999-9999-9999-9999-999999999999"), UserId = null, Name = "Rent", Type = "expense", IconName = "Home", ColorHex = "#D97706", CreatedAt = DateTime.UtcNow },
            new EasyTrack.TransactionService.Core.Entities.Category { Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), UserId = null, Name = "Water Bill", Type = "expense", IconName = "Droplet", ColorHex = "#0284C7", CreatedAt = DateTime.UtcNow },
            new EasyTrack.TransactionService.Core.Entities.Category { Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), UserId = null, Name = "Electricity Bill", Type = "expense", IconName = "Zap", ColorHex = "#EAB308", CreatedAt = DateTime.UtcNow },
            new EasyTrack.TransactionService.Core.Entities.Category { Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), UserId = null, Name = "Other", Type = "expense", IconName = "MoreHorizontal", ColorHex = "#64748B", CreatedAt = DateTime.UtcNow }
        };

        foreach (var cat in newCategories)
        {
            if (!db.Categories.Any(c => c.Id == cat.Id))
            {
                db.Categories.Add(cat);
                logger.LogInformation("Seeded new default category: {CategoryName}", cat.Name);
            }
        }
        db.SaveChanges();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not run database migrations or seeding. Ensure your database container is running.");
    }
}

app.Run();
