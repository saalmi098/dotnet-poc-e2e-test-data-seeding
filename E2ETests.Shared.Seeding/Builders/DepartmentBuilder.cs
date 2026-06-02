using WebApp.Entities;

namespace E2ETests.Shared.Seeding.Builders;

public class DepartmentBuilder : IEntityBuilder<Department>
{
    public Department Build() => Default();

    public static Department Default(Action<Department>? configure = null)
    {
        var d = new Department
        {
            Street = "Hauptstraße 42",
            City = "Vienna",
            ZipCode = "1010"
        };
        configure?.Invoke(d);
        return d;
    }
}
