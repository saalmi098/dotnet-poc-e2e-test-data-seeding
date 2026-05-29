using E2ETests.Shared.Seeding.Builders;
using Xunit;
using static Microsoft.Playwright.Assertions;
using E2ETests.ApproachA.SeedInBody.Infrastructure;

namespace E2ETests.ApproachA.SeedInBody.Tests;

public class EmployeeTests : PlaywrightTestBase
{
    [Fact]
    public async Task Employee_ShouldSeeCorrectDepartmentStreet()
    {
        var department = await Seed.SeedAsync(DepartmentBuilder.Default());
        var employee = await Seed.SeedAsync(EmployeeBuilder.Default(department.Id));

        await Page.GotoAsync($"/employees/{employee.Id}");

        await Expect(Page.Locator(".department-street"))
            .ToHaveTextAsync(department.Street);
    }

    [Fact]
    public async Task Employee_ShouldSeeCorrectCity_AfterOverride()
    {
        var department = await Seed.SeedAsync(DepartmentBuilder.Default(d => d.City = "Graz"));
        var employee = await Seed.SeedAsync(EmployeeBuilder.Default(department.Id));

        await Page.GotoAsync($"/employees/{employee.Id}");

        await Expect(Page.Locator(".department-city"))
            .ToHaveTextAsync("Graz");
    }

    [Fact]
    public async Task Employee_WithoutDepartment_ShowsNoAddress()
    {
        var employee = await Seed.SeedAsync(EmployeeBuilder.Default());

        await Page.GotoAsync($"/employees/{employee.Id}");

        await Expect(Page.Locator(".department-street"))
            .Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task Employee_UpdateName_ShouldPersistAfterSave()
    {
        var employee = await Seed.SeedAsync(EmployeeBuilder.Default());

        await Page.GotoAsync($"/employees/{employee.Id}");
        await Page.Locator("#Name").FillAsync("Updated Name");
        await Page.Locator(".save-button").ClickAsync();

        await Page.GotoAsync($"/employees/{employee.Id}");
        await Expect(Page.Locator(".employee-name")).ToHaveValueAsync("Updated Name");
    }
}

/*
TODO:

1. Customization wäre schon cool --> evtl. Dictionary in Attribute mitgeben (PropName -> Value)

    BUilder mitgeben in Seed Attribut
	    IE2ETestDataBuilderInterface etc. -> Build() Methode
	    diese Methode wird in Seed aufgerufen
	
	    Evtl. über ActionFilterAttribute custom Values mitgeben (zB. Anderer Employee Name)?? Wsl. nicht möglich wegen Mehrfachvererbung
	
    -> zB. DepartmentBUilder + EmployeeWithDepartmentBuilder Klassen

    Vorteil: Ich muss nicht jedes mal das Attribut schreiben

2. Respawn Libary anschauen für snapshot/resets der DB
3. Raw-SQL mit Sql-Server für DB Snapshots
 */
