using Bogus;
using WebApp.Entities;

namespace E2ETests.Shared.Seeding.Fakers;

/// <summary>
/// Faker for Department entities.
/// </summary>
public sealed class DepartmentFaker : Faker<Department>
{
    public DepartmentFaker()
    {
        RuleFor(d => d.Street, f => f.Address.StreetAddress())
            .RuleFor(d => d.City, f => f.Address.City())
            .RuleFor(d => d.ZipCode, f => f.Address.ZipCode());
    }

    /// <summary>
    /// Generates a Department with randomized data, optionally overridden after generation.
    /// </summary>
    public static new Department Default(Action<Department>? configure = null)
    {
        var department = new DepartmentFaker().Generate();
        configure?.Invoke(department);
        return department;
    }
}
