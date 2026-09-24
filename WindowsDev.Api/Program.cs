using AutoMapper;
using WindowsDev.Api.Mappings;
using WindowsDev.Application;
using WindowsDev.Application.Database;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.JWT;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.RegistrateApplication();
builder.Services.AddJwt(builder.Configuration);
builder.Services.RegistrateInfrastructure();
builder.Services.AddAutoMapper(cfg => { }, typeof(ApiMappingProfile));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var dbConfig = app.Services.GetRequiredService<IDatabaseConfig>();
var connectionString = builder.Configuration["ConnectionString"];
dbConfig.ConnectionString = connectionString;

app.UseHttpsRedirection();

app.MapControllers();

app.Run();