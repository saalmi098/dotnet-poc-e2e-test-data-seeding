using WebApp.Entities;

namespace E2ETests.Builders;

public static class EmployeeBuilder
{
    public static Employee Default(int? apartmentId = null, Action<Employee>? configure = null)
    {
        var uid = Guid.NewGuid().ToString("N")[..8];
        var e = new Employee
        {
            Name = $"Test Employee {uid}",
            Email = $"test_{uid}@example.com",
            ApartmentId = apartmentId
        };
        configure?.Invoke(e);
        return e;
    }
}
