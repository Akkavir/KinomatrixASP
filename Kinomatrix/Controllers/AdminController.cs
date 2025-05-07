using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Admin
    public IActionResult Index()
    {
        var users = _context.Users.ToList();
        return View(users);
    }

    // POST: /Admin/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}
