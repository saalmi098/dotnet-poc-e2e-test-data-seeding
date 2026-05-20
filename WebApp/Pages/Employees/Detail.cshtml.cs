using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Entities;

namespace WebApp.Pages.Employees;

public class DetailModel(AppDbContext db) : PageModel
{
    public Employee? Employee { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Employee = await db.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

        return Employee is null ? NotFound() : Page();
    }
}
