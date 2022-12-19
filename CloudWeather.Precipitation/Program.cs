using CloudWeather.Precipitation.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PrecipDbContext>(
  opts => { 
    opts.EnableSensitiveDataLogging();
    opts.EnableDetailedErrors();
    opts.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));
  }, ServiceLifetime.Transient
);
var app = builder.Build();

app.MapGet("/observation/{zip}", async (string zip, [FromQuery] int? days, PrecipDbContext db) => 
{
  if (days == null || days < 0 || days > 30) 
    return Results.BadRequest("Invalid 'days' parameter. Please provide a value from 1 to 30.");
  
  var startDate = DateTime.UtcNow - TimeSpan.FromDays(days.Value);
  var results = await db.Precipitation.Where(precip => precip.ZipCode == zip && precip.CreatedOn > startDate).ToListAsync();

  return Results.Ok(results);
});
app.Run();
