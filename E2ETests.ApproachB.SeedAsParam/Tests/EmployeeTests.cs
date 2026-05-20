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
    public async Task Employee_Should_See_Correct_Department_Street(SeededEmployee ctx)
    {
        await Page.GotoAsync($"/employees/{ctx.Employee.Id}");

        await Expect(Page.Locator(".department-street"))
            .ToHaveTextAsync(ctx.Department!.Street);
    }

    [Theory]
    [SeedEmployeeWithDepartment(City = "Graz")]
    public async Task Employee_Should_See_Correct_City_After_Override(SeededEmployee ctx)
    {
        await Page.GotoAsync($"/employees/{ctx.Employee.Id}");

        await Expect(Page.Locator(".department-city"))
            .ToHaveTextAsync("Graz");
    }

    [Theory]
    [SeedEmployee]
    public async Task Employee_Without_Department_Shows_No_Address(SeededEmployee ctx)
    {
        await Page.GotoAsync($"/employees/{ctx.Employee.Id}");

        await Expect(Page.Locator(".department-street"))
            .Not.ToBeVisibleAsync();
    }
}
