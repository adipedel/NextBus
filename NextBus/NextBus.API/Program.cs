using Microsoft.EntityFrameworkCore;
using NextBus.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// רישום SQLite DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=nextbus.db"));

var app = builder.Build();

// יצירת מסד הנתונים וטעינת הנתונים הראשוניים אוטומטית אם לא קיימים
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureDeleted(); // מחיקת המסד הקיים ללא הטבלה החדשה
    db.Database.EnsureCreated(); // יצירה מחדש עם כל הטבלאות (כולל LineStops)
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
};  

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
