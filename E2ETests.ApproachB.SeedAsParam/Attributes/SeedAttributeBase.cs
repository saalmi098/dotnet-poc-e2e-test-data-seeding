using E2ETests.Shared.Seeding.Infrastructure;
using E2ETests.Shared.Seeding.Seeding;
using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace E2ETests.ApproachB.SeedAsParam.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class SeedAttributeBase<TEntity> : DataAttribute
    where TEntity : class
{
    // false = skip discovery-time enumeration; async DB seed must not run at discovery
    public override bool SupportsDiscoveryEnumeration() => false;

    /// <inheritdoc/>
    public override async ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
        MethodInfo testMethod,
        DisposalTracker disposalTracker)
    {
        var seed = new SeedingContext();
        var entity = await SeedAsync(seed);
        var wrapper = new SeededEntity<TEntity>(entity, seed);
        disposalTracker.Add(wrapper);

        return [new TheoryDataRow<SeededEntity<TEntity>>(wrapper)];
    }

    /// <summary>
    /// Seeds the database and returns the seeded entity.
    /// The returned entity will be wrapped in a SeededEntity and passed as a parameter to the test method.
    /// </summary>
    /// <param name="seed">The seeding context used to seed the database.</param>
    /// <returns>The seeded entity.</returns>
    protected abstract Task<TEntity> SeedAsync(SeedingContext seed);
}
