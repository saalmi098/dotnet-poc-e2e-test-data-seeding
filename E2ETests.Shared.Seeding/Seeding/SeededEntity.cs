using E2ETests.Shared.Seeding.Infrastructure;

namespace E2ETests.Shared.Seeding.Seeding;

public sealed class SeededEntity<T>(T data, SeedingContext seed) : IAsyncDisposable
{
    public T Data { get; } = data;

    public async ValueTask DisposeAsync() => await seed.DisposeAsync();
}
