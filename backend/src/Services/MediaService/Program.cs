using Amazon.S3;
using EasyTrack.MediaService.Core.Interfaces;
using EasyTrack.MediaService.Infrastructure.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
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

// AWS S3 client setup
var serviceUrl = builder.Configuration["Aws:ServiceUrl"] ?? builder.Configuration["Aws__ServiceUrl"] ?? "http://localhost:9000";
var accessKey = builder.Configuration["Aws:AccessKey"] ?? builder.Configuration["Aws__AccessKey"] ?? "easytrack_admin";
var secretKey = builder.Configuration["Aws:SecretKey"] ?? builder.Configuration["Aws__SecretKey"] ?? "easytrack_secret_pass";

var s3Config = new AmazonS3Config
{
    ServiceURL = serviceUrl,
    ForcePathStyle = true // Required for local MinIO
};

builder.Services.AddSingleton<IAmazonS3>(sp => 
    new AmazonS3Client(accessKey, secretKey, s3Config));

// Register internal services
builder.Services.AddScoped<IS3Service, MinIoS3Service>();

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

// Ensure the bucket exists on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var s3Service = services.GetRequiredService<IS3Service>();
        var bucketName = builder.Configuration["Aws:BucketName"] ?? builder.Configuration["Aws__BucketName"] ?? "easytrack-slips";
        logger.LogInformation("Ensuring bucket '{BucketName}' exists...", bucketName);
        s3Service.EnsureBucketExistsAsync(bucketName).GetAwaiter().GetResult();
        logger.LogInformation("Bucket initialization checked successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not initialize bucket in MinIO. Make sure your MinIO container is running.");
    }
}

app.Run();
