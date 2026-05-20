using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Entities;

namespace WebApp.Pages;

public class IndexModel(AppDbContext db) : PageModel
{
    public List<Employee> Employees { get; private set; } = [];
    public List<Department> Departments { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Employees = await db.Employees
            .Include(e => e.Department)
            .OrderBy(e => e.Id)
            .ToListAsync();

        Departments = await db.Departments
            .Include(d => d.Employees)
            .OrderBy(d => d.Id)
            .ToListAsync();
    }
}
