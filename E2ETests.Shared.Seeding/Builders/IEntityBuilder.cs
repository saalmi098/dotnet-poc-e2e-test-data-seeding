namespace E2ETests.Shared.Seeding.Builders;

public interface IEntityBuilder<T> where T : class
{
    T Build();
}
