using Microsoft.EntityFrameworkCore;
using WebApp.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")
        ?? "Server=localhost,1433;Database=testdb;User Id=sa;Password=Strong!Passw0rd;TrustServerCertificate=True"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseRouting();
app.MapRazorPages();

app.Run();
