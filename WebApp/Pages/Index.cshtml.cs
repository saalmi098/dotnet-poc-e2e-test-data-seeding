using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Entities;

namespace WebApp.Pages;

public class IndexModel(AppDbContext db) : PageModel
{
    public List<Employee> Employees { get; private set; } = [];
    public List<Apartment> Apartments { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Employees = await db.Employees
            .Include(e => e.Apartment)
            .OrderBy(e => e.Id)
            .ToListAsync();

        Apartments = await db.Apartments
            .Include(a => a.Employees)
            .OrderBy(a => a.Id)
            .ToListAsync();
    }
}
