using E2ETests.Shared.Seeding.Builders;
using E2ETests.Shared.Seeding.Infrastructure;
using WebApp.Entities;

namespace E2ETests.ApproachB.SeedAsParam.Attributes;

public sealed class SeedEmployeeWithDepartmentAttribute : SeedAttributeBase<Employee>
{
    public string? City { get; set; }

    protected override async Task<Employee> SeedAsync(SeedingContext seed)
    {
        var department = await seed.SeedAsync(DepartmentBuilder.Default(d =>
        {
            if (City is not null) d.City = City;
        }));

        var employee = await seed.SeedAsync(EmployeeBuilder.Default(department.Id));
        employee.Department = department; // set navigation property for easier access in tests

        return employee;
    }
}
