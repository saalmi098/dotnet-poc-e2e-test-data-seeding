using E2ETests.Builders;
using E2ETests.Infrastructure;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace E2ETests.Tests;

public class EmployeeTests : PlaywrightTestBase
{
    [Fact]
    public async Task Employee_Should_See_Correct_Department_Street()
    {
        var department = await Seed.SeedAsync(DepartmentBuilder.Default());
        var employee = await Seed.SeedAsync(EmployeeBuilder.Default(department.Id));

        await Page.GotoAsync($"/employees/{employee.Id}");

        await Expect(Page.Locator(".department-street"))
            .ToHaveTextAsync(department.Street);
    }

    [Fact]
    public async Task Employee_Should_See_Correct_City_After_Override()
    {
        var department = await Seed.SeedAsync(DepartmentBuilder.Default(d => d.City = "Graz"));
        var employee = await Seed.SeedAsync(EmployeeBuilder.Default(department.Id));

        await Page.GotoAsync($"/employees/{employee.Id}");

        await Expect(Page.Locator(".department-city"))
            .ToHaveTextAsync("Graz");
    }

    [Fact]
    public async Task Employee_Without_Department_Shows_No_Address()
    {
        var employee = await Seed.SeedAsync(EmployeeBuilder.Default());

        await Page.GotoAsync($"/employees/{employee.Id}");

        await Expect(Page.Locator(".department-street"))
            .Not.ToBeVisibleAsync();
    }
}
