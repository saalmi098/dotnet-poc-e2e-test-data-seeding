using E2ETests.Shared.Seeding.Infrastructure;
using WebApp.Entities;

namespace E2ETests.Shared.Seeding.Builders;

public class EmployeeBuilder : IEntityBuilder<Employee>
{
    public Task<Employee> SeedAsync(SeedingContext seed)
        => seed.SeedAsync(Default());

    public static Employee Default(int? departmentId = null, Action<Employee>? configure = null)
    {
        var uid = Guid.NewGuid().ToString("N")[..8];
        var e = new Employee
        {
            Name = $"Test Employee {uid}",
            Email = $"test_{uid}@example.com",
            DepartmentId = departmentId
        };

        configure?.Invoke(e);
        return e;
    }
}
