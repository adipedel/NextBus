using Microsoft.EntityFrameworkCore;
using NextBus.API.Data;
using NextBus.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<RealtimeService>();
builder.Services.AddScoped<GtfsImporterService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();
builder.Services.AddScoped<RealtimeService>();

// רישום SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=nextbus.db"));

var app = builder.Build();

// אתחול מסד נתונים וטעינה
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    var importer = scope.ServiceProvider.GetRequiredService<GtfsImporterService>();
    await importer.ImportTelAvivDataAsync(app.Environment.ContentRootPath);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();