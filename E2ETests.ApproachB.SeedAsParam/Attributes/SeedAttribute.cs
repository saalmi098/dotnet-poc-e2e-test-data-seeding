using E2ETests.Shared.Seeding.Builders;
using E2ETests.Shared.Seeding.Infrastructure;

namespace E2ETests.ApproachB.SeedAsParam.Attributes;

public sealed class SeedAttribute<TBuilder, TEntity> : SeedAttributeBase<TEntity>
    where TBuilder : IEntityBuilder<TEntity>, new()
    where TEntity : class
{
    protected override Task<TEntity> SeedAsync(SeedingContext seed) =>
        new TBuilder().SeedAsync(seed);
}
