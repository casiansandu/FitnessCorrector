using FitnessCorrector.Infrastructure.Persistence;
using FitnessCorrector.Infrastructure.Repositories;
using FitnessCorrector.Infrastructure.Services;
using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Exercises.Commands;
using FitnessCorrector.Application.Common.Behaviors;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text;
using ClassLibrary1.Services;
using FitnessCorrector.Infrastructure.Repositories.UsersRepository;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using DotNetEnv;

var envPaths = new List<string>
{
    Path.Combine(Directory.GetCurrentDirectory(), ".env")
};

var currentDirectory = Directory.GetCurrentDirectory();
for (var i = 0; i < 6; i++)
{
    var parentDirectory = Directory.GetParent(currentDirectory)?.FullName;
    if (string.IsNullOrWhiteSpace(parentDirectory))
    {
        break;
    }

    envPaths.Add(Path.Combine(parentDirectory, ".env"));
    currentDirectory = parentDirectory;
}

var envPath = envPaths.FirstOrDefault(File.Exists);
if (!string.IsNullOrWhiteSpace(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("JwtSettings:Secret is missing. Make sure ServerFitnessCorrector/.env is present and loaded before startup.");
}

// 1. Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
            "https://fitness-corrector-frontend.vercel.app",
            "https://fitness-corrector-frontend-git-main-casiansandus-projects.vercel.app")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Allow credentials (cookies, authorization headers, etc.)
    });
});

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateExerciseCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateExerciseCommand).Assembly);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IWorkoutSessionRepository, WorkoutSessionsRepository>();
builder.Services.AddScoped<IWorkoutSessionMetricsRepository, WorkoutSessionMetricsRepository>();
builder.Services.AddScoped<IExercisesRepository, ExercisesRepository>();
builder.Services.AddScoped<IAiAnalyzerService, PythonAiAnalyzerService>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ISubscriptionService, StripeSubscriptionService>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Read token from HttpOnly cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Try to get token from cookie first, then fall back to Authorization header
            var token = context.Request.Cookies["fitnessCorrectorToken"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed admin user
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DbSeeder.SeedAdminUserAsync(context, configuration);
    await DbSeeder.SeedExercisesAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. USE CORS (Must be before MapControllers and Authorization)
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
// programul asteapta scriptul de python sa se termine -> poate astepta mult pentru videouri mai lungi(async?)
// stocarea locala a videourilor ocupa spatiu(cloud storage?)
// daca multi utilizatori analizeaza videouri se umple memoria ram din cauza transferului de videouri