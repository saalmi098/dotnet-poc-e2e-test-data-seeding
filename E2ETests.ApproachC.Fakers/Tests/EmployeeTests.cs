using E2ETests.ApproachC.Fakers.Infrastructure;
using E2ETests.Shared.Seeding.Fakers;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace E2ETests.ApproachC.Fakers.Tests;

public class EmployeeTests : PlaywrightTestBase
{
    [Fact]
    public async Task Employee_ShouldSeeCorrectDepartmentStreet()
    {
        // Department not seeded separately: EmployeeFaker.Default() auto-generates one via the
        // Department navigation property, and SeedAsync inserts both in a single SaveChanges call.
        var employee = await Seed.SeedAsync(EmployeeFaker.Default());

        await Page.GotoAsync($"/employees/{employee.Id}");

        await Expect(Page.Locator(".department-street"))
            .ToHaveTextAsync(employee.Department!.Street);
    }

    [Fact]
    public async Task Employee_ShouldSeeCorrectCity_AfterOverride()
    {
        var department = DepartmentFaker.Default(d => d.City = "Graz");
        var employee = await Seed.SeedAsync(EmployeeFaker.Default(department));

        await Page.GotoAsync($"/employees/{employee.Id}");

        await Expect(Page.Locator(".department-city"))
            .ToHaveTextAsync("Graz");
    }

    [Fact]
    public async Task Employee_WithoutDepartment_ShowsNoAddress()
    {
        var employee = await Seed.SeedAsync(EmployeeFaker.WithoutDepartment());

        await Page.GotoAsync($"/employees/{employee.Id}");

        await Expect(Page.Locator(".department-street"))
            .Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task Employee_UpdateName_ShouldPersistAfterSave()
    {
        var employee = await Seed.SeedAsync(EmployeeFaker.WithoutDepartment());

        await Page.GotoAsync($"/employees/{employee.Id}");
        await Page.Locator("#Name").FillAsync("Updated Name");
        await Page.Locator(".save-button").ClickAsync();

        await Page.GotoAsync($"/employees/{employee.Id}");
        await Expect(Page.Locator(".employee-name")).ToHaveValueAsync("Updated Name");
    }
}
