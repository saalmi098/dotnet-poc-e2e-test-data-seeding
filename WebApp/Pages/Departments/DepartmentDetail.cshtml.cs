using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Entities;

namespace WebApp.Pages.Departments;

public class DepartmentDetailModel(AppDbContext db) : PageModel
{
    public Department? Department { get; private set; }

    [BindProperty] public string Street { get; set; } = "";
    [BindProperty] public string City { get; set; } = "";
    [BindProperty] public string ZipCode { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is not null)
        {
            Department = await db.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (Department is null) return NotFound();

            Street = Department.Street;
            City = Department.City;
            ZipCode = Department.ZipCode;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            var department = new Department { Street = Street, City = City, ZipCode = ZipCode };
            db.Departments.Add(department);
        }
        else
        {
            var department = await db.Departments.FindAsync(id);
            if (department is null) return NotFound();

            department.Street = Street;
            department.City = City;
            department.ZipCode = ZipCode;
        }

        await db.SaveChangesAsync();
        return RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var department = await db.Departments.FindAsync(id);
        if (department is not null)
        {
            db.Departments.Remove(department);
            await db.SaveChangesAsync();
        }

        return RedirectToPage("/Index");
    }
}
