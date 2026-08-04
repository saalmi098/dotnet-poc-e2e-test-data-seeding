using Bogus;
using WebApp.Entities;

namespace E2ETests.Shared.Seeding.Fakers;

/// <summary>
/// Faker for Employee entities.
/// </summary>
public sealed class EmployeeFaker : Faker<Employee>
{
    /// <param name="departmentId">Department the employee belongs to, if any.</param>
    public EmployeeFaker(int? departmentId = null)
    {
        RuleFor(e => e.Name, f => f.Name.FullName())
            .RuleFor(e => e.Email, (f, e) => f.Internet.Email(e.Name))
            .RuleFor(e => e.DepartmentId, () => departmentId);
    }

    /// <summary>
    /// Generates an Employee with randomized data, optionally overridden after generation.
    /// </summary>
    public static new Employee Default(int? departmentId = null, Action<Employee>? configure = null)
    {
        var employee = new EmployeeFaker(departmentId).Generate();
        configure?.Invoke(employee);
        return employee;
    }
}
