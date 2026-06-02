using E2ETests.Shared.Seeding.Infrastructure;
using WebApp.Entities;

namespace E2ETests.Shared.Seeding.Builders;

public class EmployeeWithDepartmentBuilder : IEntityBuilder<Employee>
{
    public Action<Department>? ConfigureDepartment { get; set; }
    public Action<Employee>? ConfigureEmployee { get; set; }

    public async Task<Employee> SeedAsync(SeedingContext seed)
    {
        var department = await seed.SeedAsync(DepartmentBuilder.Default(ConfigureDepartment));

        var employee = await seed.SeedAsync(EmployeeBuilder.Default(null, e =>
        {
            ConfigureEmployee?.Invoke(e);

            e.DepartmentId = department.Id;
            e.Department = department;
        }));
        
        return employee;
    }
}
