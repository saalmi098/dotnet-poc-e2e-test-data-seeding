using E2ETests.ApproachB.SeedAsParam.Attributes;
using E2ETests.ApproachB.SeedAsParam.Infrastructure;
using E2ETests.Shared.Seeding.Builders;
using E2ETests.Shared.Seeding.Seeding;
using WebApp.Entities;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace E2ETests.ApproachB.SeedAsParam.Tests;

public class EmployeeTests : PlaywrightTestBase
{
    // disadvantage when we dont use SeedMemberData: we need to duplicate the properties we want to customize in the builder
    private static EmployeeWithDepartmentBuilder GrazEmployee =>
        new() { ConfigureDepartment = d => { d.City = "Graz"; d.ZipCode = "8010"; } };

    [Theory]
    [Seed<EmployeeWithDepartmentBuilder, Employee>]
    public async Task Employee_ShouldSeeCorrectDepartmentStreet(SeededEntity<Employee> ctx)
    {
        await Page.GotoAsync($"/employees/{ctx.Data.Id}");

        await Expect(Page.Locator(".department-street"))
            .ToHaveTextAsync(ctx.Data.Department!.Street);
    }

    [Theory]
    [SeedMemberData<Employee>(nameof(GrazEmployee))]
    public async Task Employee_ShouldSeeCorrectCity_AfterOverride(SeededEntity<Employee> ctx)
    {
        await Page.GotoAsync($"/employees/{ctx.Data.Id}");

        await Expect(Page.Locator(".department-city"))
            .ToHaveTextAsync("Graz");
    }

    [Theory]
    [Seed<EmployeeBuilder, Employee>]
    public async Task Employee_WithoutDepartment_ShowsNoAddress(SeededEntity<Employee> ctx)
    {
        await Page.GotoAsync($"/employees/{ctx.Data.Id}");

        await Expect(Page.Locator(".department-street"))
            .Not.ToBeVisibleAsync();
    }

    [Theory]
    [Seed<EmployeeBuilder, Employee>]
    public async Task Employee_UpdateName_ShouldPersistAfterSave(SeededEntity<Employee> ctx)
    {
        await Page.GotoAsync($"/employees/{ctx.Data.Id}");
        await Page.Locator("#Name").FillAsync("Updated Name");
        await Page.Locator(".save-button").ClickAsync();

        await Page.GotoAsync($"/employees/{ctx.Data.Id}");
        await Expect(Page.Locator(".employee-name")).ToHaveValueAsync("Updated Name");
    }
}
