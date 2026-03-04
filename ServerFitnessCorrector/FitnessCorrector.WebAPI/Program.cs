using FitnessCorrector.Infrastructure.Persistence;
using FitnessCorrector.Infrastructure.Repositories;
using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Exercises.Commands;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using ClassLibrary1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateExerciseCommand).Assembly));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("FitnessDb"));
builder.Services.AddScoped<IWorkoutSessionRepository, WorkoutSessionsRepository>();
builder.Services.AddScoped<IExercisesRepository, ExercisesRepository>();
builder.Services.AddScoped<IAiAnalyzerService, PythonAiAnalyzerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
// programul asteapta scriptul de python sa se termine -> poate astepta mult pentru videouri mai lungi(async?)
// stocarea locala a videourilor ocupa spatiu()(cloud storage?)
// daca multi utilizatori analizeaza videouri se umple memoria ram din cauza transferului de videouri()