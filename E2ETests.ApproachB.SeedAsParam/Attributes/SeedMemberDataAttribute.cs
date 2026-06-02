using E2ETests.Shared.Seeding.Builders;
using E2ETests.Shared.Seeding.Infrastructure;
using E2ETests.Shared.Seeding.Seeding;
using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace E2ETests.ApproachB.SeedAsParam.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class SeedMemberDataAttribute<TEntity>(string memberName) : DataAttribute
    where TEntity : class
{
    // false = skip discovery-time enumeration; async DB seed must not run at discovery
    public override bool SupportsDiscoveryEnumeration() => false;

    public override async ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
        MethodInfo testMethod,
        DisposalTracker disposalTracker)
    {
        var builders = ResolveBuilders(testMethod.DeclaringType!);
        var rows = new List<ITheoryDataRow>();

        foreach (var builder in builders)
        {
            var seed = new SeedingContext();
            var entity = await builder.SeedAsync(seed);
            var wrapper = new SeededEntity<TEntity>(entity, seed);
            disposalTracker.Add(wrapper);
            rows.Add(new TheoryDataRow<SeededEntity<TEntity>>(wrapper));
        }

        return rows;
    }

    private IEnumerable<IEntityBuilder<TEntity>> ResolveBuilders(Type testClass)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        var value = testClass.GetProperty(memberName, flags)?.GetValue(null)
                 ?? testClass.GetMethod(memberName, flags)?.Invoke(null, [])
                 ?? throw new InvalidOperationException(
                        $"Static member '{memberName}' not found on '{testClass.Name}'.");

        return value switch
        {
            IEntityBuilder<TEntity> single            => [single],
            IEnumerable<IEntityBuilder<TEntity>> many => many,
            _ => throw new InvalidOperationException(
                     $"'{memberName}' must return IEntityBuilder<{typeof(TEntity).Name}> " +
                     $"or IEnumerable<IEntityBuilder<{typeof(TEntity).Name}>>.")
        };
    }
}
