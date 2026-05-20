using Microsoft.EntityFrameworkCore;
using WebApp.Entities;

namespace WebApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Apartment> Apartments => Set<Apartment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Apartment)
            .WithMany(a => a.Employees)
            .HasForeignKey(e => e.ApartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
