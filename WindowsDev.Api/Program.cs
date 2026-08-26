using WindowsDev.Application;
using WindowsDev.Application.DatabaseInterfaces;
using WindowsDev.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.RegistrateApplication();
builder.Services.RegistrateInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var dbConfig = app.Services.GetRequiredService<IDatabaseConfig>();
dbConfig.ConnectionString = "Host=localhost;Port=5432;Database=WindowsDev;Username=postgres;Password=q29384756";

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();