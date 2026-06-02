using E2ETests.Shared.Seeding.Builders;
using E2ETests.Shared.Seeding.Infrastructure;
using E2ETests.Shared.Seeding.Seeding;
using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace E2ETests.ApproachB.SeedAsParam.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class SeedAttribute<TBuilder, TEntity> : DataAttribute
    where TBuilder : IEntityBuilder<TEntity>, new()
    where TEntity : class
{
    // false = skip discovery-time enumeration; async DB seed must not run at discovery
    public override bool SupportsDiscoveryEnumeration() => false;

    public override async ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
        MethodInfo testMethod,
        DisposalTracker disposalTracker)
    {
        var seed = new SeedingContext();
        var entity = await seed.SeedAsync(new TBuilder().Build());

        var wrapper = new SeededEntity<TEntity>(entity, seed);
        disposalTracker.Add(wrapper);

        return [new TheoryDataRow<SeededEntity<TEntity>>(wrapper)];
    }
}
