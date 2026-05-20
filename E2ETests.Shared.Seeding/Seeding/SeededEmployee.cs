using E2ETests.Shared.Seeding.Infrastructure;
using WebApp.Entities;

namespace E2ETests.Shared.Seeding.Seeding;

public sealed class SeededEmployee : IAsyncDisposable
{
    public Employee Employee { get; }
    public Department? Department { get; }

    private readonly SeedingContext _seed;

    public SeededEmployee(Employee employee, Department? department, SeedingContext seed)
    {
        Employee = employee;
        Department = department;
        _seed = seed;
    }

    public async ValueTask DisposeAsync() => await _seed.DisposeAsync();
}
