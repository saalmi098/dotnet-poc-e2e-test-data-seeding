using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using WebApp.Data;
using WebApp.Entities;
using Xunit;

namespace E2ETests.ApproachC.Testcontainers.Infrastructure;

public sealed class E2EFixture : IAsyncLifetime
{
    private MsSqlContainer _container = null!;
    private WebAppFactory _factory = null!;

    public string BaseUrl { get; private set; } = "";
    public string ConnectionString { get; private set; } = "";

    public async ValueTask InitializeAsync()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        await _container.StartAsync();

        ConnectionString = _container.GetConnectionString();
        _factory = new WebAppFactory(ConnectionString);

        // Accessing Services triggers WebApplicationFactory startup:
        // Program.cs runs → EnsureCreatedAsync creates schema in container
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedPreFilledDataAsync(db);

        BaseUrl = _factory.BaseUrl;
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    private static async Task SeedPreFilledDataAsync(AppDbContext db)
    {
        db.Departments.AddRange(
            new Department { Street = "Main Street", City = "Springfield", ZipCode = "12345" },
            new Department { Street = "Elm Street",  City = "Shelbyville", ZipCode = "54321" },
            new Department { Street = "Oak Avenue",  City = "Ogdenville",  ZipCode = "67890" }
        );
        await db.SaveChangesAsync();

        var depts = db.Departments.Local.ToList();
        db.Employees.AddRange(
            new Employee { Name = "John Doe",      Email = "john.doe@example.com",      DepartmentId = depts[0].Id },
            new Employee { Name = "Jane Smith",    Email = "jane.smith@example.com",    DepartmentId = depts[1].Id },
            new Employee { Name = "Alice Johnson", Email = "alice.johnson@example.com", DepartmentId = depts[2].Id }
        );
        await db.SaveChangesAsync();
    }
}

[CollectionDefinition("E2E")]
public class E2ECollection : ICollectionFixture<E2EFixture> { }
