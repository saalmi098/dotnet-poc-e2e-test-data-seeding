using E2ETests.Shared.Seeding.Builders;
using E2ETests.Shared.Seeding.Infrastructure;
using E2ETests.Shared.Seeding.Seeding;
using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace E2ETests.ApproachB.SeedAsParam.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class SeedEmployeeWithDepartmentAttribute : DataAttribute
{
    public string? City { get; set; }

    // false = skip discovery-time enumeration; async DB seed must not run at discovery
    public override bool SupportsDiscoveryEnumeration() => false;

    /// <inheritdoc/>
    public override async ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
        MethodInfo testMethod,
        DisposalTracker disposalTracker)
    {
        var seed = new SeedingContext();
        var department = await seed.SeedAsync(
            DepartmentBuilder.Default(d =>
            {
                if (City is not null) d.City = City;
            }));
        var employee = await seed.SeedAsync(EmployeeBuilder.Default(department.Id));

        var wrapper = new SeededEntity<EmployeeWithDepartment>(new(employee, department), seed);
        disposalTracker.Add(wrapper);

        return [new TheoryDataRow<SeededEntity<EmployeeWithDepartment>>(wrapper)];
    }
}
