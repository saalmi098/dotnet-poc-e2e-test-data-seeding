using E2ETests.Shared.Seeding.Infrastructure;
using WebApp.Entities;

namespace E2ETests.Shared.Seeding.Seeding;

public sealed class SeededEmployee(Employee employee, Department? department, SeedingContext seed) : IAsyncDisposable
{
    public Employee Employee { get; } = employee;
    public Department? Department { get; } = department;

    public async ValueTask DisposeAsync() => await seed.DisposeAsync();
}
