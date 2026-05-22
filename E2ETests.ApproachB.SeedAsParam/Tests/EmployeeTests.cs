using E2ETests.ApproachB.SeedAsParam.Attributes;
using E2ETests.ApproachB.SeedAsParam.Infrastructure;
using E2ETests.Shared.Seeding.Seeding;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace E2ETests.ApproachB.SeedAsParam.Tests;

public class EmployeeTests : PlaywrightTestBase
{
    [Theory]
    [SeedEmployeeWithDepartment]
    public async Task Employee_ShouldSeeCorrectDepartmentStreet(SeededEmployee ctx)
    {
        await Page.GotoAsync($"/employees/{ctx.Employee.Id}");

        await Expect(Page.Locator(".department-street"))
            .ToHaveTextAsync(ctx.Department!.Street);
    }

    [Theory]
    [SeedEmployeeWithDepartment(City = "Graz")]
    public async Task Employee_ShouldSeeCorrectCity_AfterOverride(SeededEmployee ctx)
    {
        await Page.GotoAsync($"/employees/{ctx.Employee.Id}");

        await Expect(Page.Locator(".department-city"))
            .ToHaveTextAsync("Graz");
    }

    [Theory]
    [SeedEmployee]
    public async Task Employee_WithoutDepartment_ShowsNoAddress(SeededEmployee ctx)
    {
        await Page.GotoAsync($"/employees/{ctx.Employee.Id}");

        await Expect(Page.Locator(".department-street"))
            .Not.ToBeVisibleAsync();
    }

    [Theory]
    [SeedEmployee]
    public async Task Employee_UpdateName_ShouldPersistAfterSave(SeededEmployee ctx)
    {
        await Page.GotoAsync($"/employees/{ctx.Employee.Id}");
        await Page.Locator("#Name").FillAsync("Updated Name");
        await Page.Locator(".save-button").ClickAsync();

        await Page.GotoAsync($"/employees/{ctx.Employee.Id}");
        await Expect(Page.Locator(".employee-name")).ToHaveValueAsync("Updated Name");
    }
}
