using E2ETests.Builders;
using E2ETests.Infrastructure;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace E2ETests.Tests;

public class EmployeeTests : PlaywrightTestBase
{
    [Fact]
    public async Task Employee_Should_See_Correct_Apartment_Street()
    {
        var apartment = await Seed.SeedAsync(ApartmentBuilder.Default());
        var employee = await Seed.SeedAsync(EmployeeBuilder.Default(apartment.Id));

        await Page.GotoAsync($"/employees/{employee.Id}");

        await Expect(Page.Locator(".apartment-street"))
            .ToHaveTextAsync(apartment.Street);
    }

    [Fact]
    public async Task Employee_Should_See_Correct_City_After_Override()
    {
        var apartment = await Seed.SeedAsync(ApartmentBuilder.Default(a => a.City = "Graz"));
        var employee = await Seed.SeedAsync(EmployeeBuilder.Default(apartment.Id));

        await Page.GotoAsync($"/employees/{employee.Id}");

        await Expect(Page.Locator(".apartment-city"))
            .ToHaveTextAsync("Graz");
    }

    [Fact]
    public async Task Employee_Without_Apartment_Shows_No_Address()
    {
        var employee = await Seed.SeedAsync(EmployeeBuilder.Default());

        await Page.GotoAsync($"/employees/{employee.Id}");

        await Expect(Page.Locator(".apartment-street"))
            .Not.ToBeVisibleAsync();
    }
}
