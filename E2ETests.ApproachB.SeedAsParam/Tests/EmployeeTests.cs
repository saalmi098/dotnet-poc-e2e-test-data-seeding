using E2ETests.ApproachB.SeedAsParam.Attributes;
using E2ETests.ApproachB.SeedAsParam.Infrastructure;
using E2ETests.Shared.Seeding.Seeding;
using Microsoft.Playwright;
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

    [Theory]
    [SeedEmployeeWithDepartment]
    public async Task Employee_FullCrudFlow_ListCreateUpdateDelete(SeededEmployee ctx)
    {
        var uid = Guid.NewGuid().ToString("N")[..6];
        var newName = $"E2E Employee {uid}";
        var newEmail = $"e2e_{uid}@example.com";
        var updatedName = $"E2E Updated {uid}";

        // LIST: seeded employee appears in overview
        await Page.GotoAsync("/");
        await Expect(Page.Locator("td.employee-name").Filter(new LocatorFilterOptions { HasText = ctx.Employee.Name }))
            .ToBeVisibleAsync();

        // CREATE: fill new-employee form and submit (no department yet)
        await Page.Locator("a[href='/employees']").ClickAsync();
        await Expect(Page.Locator("h1")).ToHaveTextAsync("New Employee");
        await Expect(Page.Locator(".save-button")).ToHaveTextAsync("Create");

        await Page.Locator("#Name").FillAsync(newName);
        await Page.Locator("#Email").FillAsync(newEmail);
        await Page.Locator(".save-button").ClickAsync();

        // Back on index: new employee visible with correct name and email, no department
        await Expect(Page.Locator("h1")).ToHaveTextAsync("POC — Test Data Seeding");
        var newRow = Page.Locator("tbody tr").Filter(new LocatorFilterOptions { HasText = newName });
        await Expect(newRow).ToBeVisibleAsync();
        await Expect(newRow.Locator("td.employee-email")).ToHaveTextAsync(newEmail);

        // UPDATE: navigate to detail via row click, verify pre-filled values, change name + assign department
        await newRow.ClickAsync();
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Edit Employee");
        await Expect(Page.Locator("#Name")).ToHaveValueAsync(newName);
        await Expect(Page.Locator("#Email")).ToHaveValueAsync(newEmail);

        var detailUrl = Page.Url;

        await Page.Locator("#Name").FillAsync(updatedName);
        await Page.Locator("#DepartmentId").SelectOptionAsync(ctx.Department!.Id.ToString());
        await Page.Locator(".save-button").ClickAsync();

        // Back on index: updated name visible, old name gone
        await Expect(Page.Locator("td.employee-name").Filter(new LocatorFilterOptions { HasText = updatedName }))
            .ToBeVisibleAsync();
        await Expect(Page.Locator("td.employee-name").Filter(new LocatorFilterOptions { HasText = newName }))
            .Not.ToBeVisibleAsync();

        // Navigate back to detail: verify updated name and department info displayed
        await Page.GotoAsync(detailUrl);
        await Expect(Page.Locator("#Name")).ToHaveValueAsync(updatedName);
        await Expect(Page.Locator(".department-street")).ToHaveTextAsync(ctx.Department.Street);
        await Expect(Page.Locator(".department-city")).ToHaveTextAsync(ctx.Department.City);

        // DELETE: accept confirm dialog, delete
        Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
        await Page.Locator(".delete-button").ClickAsync();

        // Back on index: deleted employee no longer listed
        await Expect(Page.Locator("h1")).ToHaveTextAsync("POC — Test Data Seeding");
        await Expect(Page.Locator("td.employee-name").Filter(new LocatorFilterOptions { HasText = updatedName }))
            .Not.ToBeVisibleAsync();
    }
}
