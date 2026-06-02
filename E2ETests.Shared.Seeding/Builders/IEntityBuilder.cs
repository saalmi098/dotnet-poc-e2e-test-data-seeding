using E2ETests.Shared.Seeding.Infrastructure;

namespace E2ETests.Shared.Seeding.Builders;

public interface IEntityBuilder<T> where T : class
{
    Task<T> SeedAsync(SeedingContext seed);
}
