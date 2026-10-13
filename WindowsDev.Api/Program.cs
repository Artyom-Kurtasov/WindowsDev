using WindowsDev.Api.Mappings;
using WindowsDev.Application;
using WindowsDev.Application.Database;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.JWT;
using WindowsDev.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.RegistrateApplication();
builder.Services.AddJwt(builder.Configuration);
builder.Services.RegistrateInfrastructure();
builder.Services.RegistrateLogging();
builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<ApiMappingProfile>(); });

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