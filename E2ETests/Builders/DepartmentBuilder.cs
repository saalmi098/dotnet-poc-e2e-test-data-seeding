using WebApp.Entities;

namespace E2ETests.Builders;

public static class DepartmentBuilder
{
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
