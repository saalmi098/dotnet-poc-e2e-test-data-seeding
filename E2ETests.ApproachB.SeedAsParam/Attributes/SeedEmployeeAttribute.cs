using E2ETests.Shared.Seeding.Builders;
using E2ETests.Shared.Seeding.Infrastructure;
using E2ETests.Shared.Seeding.Seeding;
using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace E2ETests.ApproachB.SeedAsParam.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class SeedEmployeeAttribute : DataAttribute
{
    // false = skip discovery-time enumeration; async DB seed must not run at discovery
    public override bool SupportsDiscoveryEnumeration() => false;

    /// <inheritdoc/>
    public override async ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
        MethodInfo testMethod,
        DisposalTracker disposalTracker)
    {
        var seed = new SeedingContext();
        var employee = await seed.SeedAsync(EmployeeBuilder.Default());

        var wrapper = new SeededEmployee(employee, null, seed);
        disposalTracker.Add(wrapper); // TODO: what happens when this line is missing? The seed still gets disposed through SeededEmployee.DisposeAsync

        return [new TheoryDataRow<SeededEmployee>(wrapper)]; // TODO: warning: wrapper is not Serializable
    }
}
