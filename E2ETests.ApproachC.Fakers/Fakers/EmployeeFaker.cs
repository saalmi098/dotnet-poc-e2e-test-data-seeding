using Bogus;
using WebApp.Entities;

namespace E2ETests.Shared.Seeding.Fakers;

/// <summary>
/// Faker for Employee entities.
/// </summary>
/// <remarks>
/// Sets the Department navigation property instead of DepartmentId. A freshly generated
/// (unsaved) Department therefore becomes part of the employee's object graph, so
/// SeedingContext.SeedAsync(employee) alone inserts both rows and lets EF Core fix up the
/// foreign key - no separate department seed/save round-trip needed to obtain its Id.
/// </remarks>
public sealed class EmployeeFaker : Faker<Employee>
{
    /// <param name="department">Department the employee belongs to, or null for no department.</param>
    public EmployeeFaker(Department? department)
    {
        RuleFor(e => e.Name, f => f.Name.FullName())
            .RuleFor(e => e.Email, (f, e) => f.Internet.Email(e.Name))
            .RuleFor(e => e.Department, () => department);
    }

    /// <summary>
    /// Generates an Employee with randomized data and a default Department (auto-generated unless one is supplied),
    /// optionally overridden after generation.
    /// </summary>
    public static new Employee Default(Department? department = null, Action<Employee>? configure = null)
    {
        var employee = new EmployeeFaker(department ?? DepartmentFaker.Default()).Generate();
        configure?.Invoke(employee);
        return employee;
    }

    /// <summary>
    /// Generates an Employee with randomized data and no Department, optionally overridden after generation.
    /// </summary>
    public static Employee WithoutDepartment(Action<Employee>? configure = null)
    {
        var employee = new EmployeeFaker(null).Generate();
        configure?.Invoke(employee);
        return employee;
    }
}
