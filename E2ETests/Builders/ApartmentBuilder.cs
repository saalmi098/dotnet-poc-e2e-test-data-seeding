using WebApp.Entities;

namespace E2ETests.Builders;

public static class ApartmentBuilder
{
    public static Apartment Default(Action<Apartment>? configure = null)
    {
        var a = new Apartment
        {
            Street = "Hauptstraße 42",
            City = "Vienna",
            ZipCode = "1010"
        };
        configure?.Invoke(a);
        return a;
    }
}
