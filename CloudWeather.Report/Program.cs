using CloudWeather.Report.BusinessLogic;
using CloudWeather.Report.Config;
using CloudWeather.Report.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WeatherReportDbContext>(
  opts => { 
    opts.EnableSensitiveDataLogging();
    opts.EnableDetailedErrors();
    opts.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));
  }, ServiceLifetime.Transient
);

builder.Services.AddHttpClient();
builder.Services.AddOptions();
builder.Services.AddTransient<IWeatherReportAggregator, WeatherReportAggregator>();
builder.Services.Configure<WeatherDataConfig>(builder.Configuration.GetSection("WeatherDataConfig"));

var app = builder.Build();

app.MapGet("/weather-report/{zip}", 
  async (string zip, [FromQuery] int? days, IWeatherReportAggregator weatherAgg) => { 
     if (days == null || days < 0 || days > 30) 
    return Results.BadRequest("Invalid 'days' parameter. Please provide a value from 1 to 30.");
    var report = await weatherAgg.BuildReport(zip, days.Value);
    return Results.Ok(report);
  }
);
app.Run();
