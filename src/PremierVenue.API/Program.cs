using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PremierVenue.API.Services;
using PremierVenue.Core.Services;
using PremierVenue.Core.Validators;
using PremierVenue.Core.DTOs;
using PremierVenue.Infrastructure.Data;
using PremierVenue.Infrastructure.Repositories;
using PremierVenue.Domain.Interfaces;
using PremierVenue.Domain.Entities;
using FluentValidation;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Database configuration
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseInMemoryDatabase("PremierVenueDb");
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// Repository registration
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IVenueRepository, VenueRepository>();
builder.Services.AddScoped<ISavedVenueRepository, SavedVenueRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Service registration
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<ISavedVenueService, SavedVenueService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddHostedService<BookingLifecycleHostedService>();

// Validator registration
builder.Services.AddScoped<IValidator<CreateUserDto>, CreateUserDtoValidator>();
builder.Services.AddScoped<IValidator<CreateStaffInvitationDto>, CreateStaffInvitationDtoValidator>();
builder.Services.AddScoped<IValidator<CreateVenueDto>, CreateVenueDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateVenueDto>, UpdateVenueDtoValidator>();
builder.Services.AddScoped<IValidator<CreateBookingDto>, CreateBookingDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateBookingDto>, UpdateBookingDtoValidator>();
builder.Services.AddScoped<IValidator<BookingStatusUpdateDto>, BookingStatusUpdateDtoValidator>();
builder.Services.AddScoped<IValidator<BookingQuoteDto>, BookingQuoteDtoValidator>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PremierVenue API",
        Version = "v1",
        Description = "Event Booking System API"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize the database and seed data. Run Data/initial.sql first for a SQL Server database.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        if (context.Database.IsRelational())
        {
            context.Database.ExecuteSqlRaw("IF COL_LENGTH('Venues', 'IsFeatured') IS NULL ALTER TABLE [Venues] ADD [IsFeatured] bit NOT NULL CONSTRAINT [DF_Venues_IsFeatured] DEFAULT 0");
        }
        DataSeeder.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error initializing database and seeding data");
    }
}

app.Run();
