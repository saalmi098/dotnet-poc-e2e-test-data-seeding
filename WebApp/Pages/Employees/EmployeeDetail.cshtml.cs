using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Entities;

namespace WebApp.Pages.Employees;

public class EmployeeDetailModel(AppDbContext db) : PageModel
{
    public Employee? Employee { get; private set; }
    public List<SelectListItem> Departments { get; private set; } = [];

    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public int? DepartmentId { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is not null)
        {
            Employee = await db.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (Employee is null) return NotFound();

            Name = Employee.Name;
            Email = Employee.Email;
            DepartmentId = Employee.DepartmentId;
        }

        await LoadDepartmentsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            var employee = new Employee { Name = Name, Email = Email, DepartmentId = DepartmentId };
            db.Employees.Add(employee);
        }
        else
        {
            var employee = await db.Employees.FindAsync(id);
            if (employee is null) return NotFound();

            employee.Name = Name;
            employee.Email = Email;
            employee.DepartmentId = DepartmentId;
        }

        await db.SaveChangesAsync();
        return RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee is not null)
        {
            db.Employees.Remove(employee);
            await db.SaveChangesAsync();
        }

        return RedirectToPage("/Index");
    }

    private async Task LoadDepartmentsAsync()
    {
        var departments = await db.Departments.OrderBy(d => d.City).ToListAsync();
        Departments = [.. departments.Select(d => new SelectListItem($"{d.City} — {d.Street}", d.Id.ToString()))];
    }
}
