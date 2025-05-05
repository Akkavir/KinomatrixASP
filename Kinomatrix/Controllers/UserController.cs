using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kinomatrix.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Profile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var interactions = _context.MovieInteractions
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.DateTime)
                .ToList();

            return View(interactions);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var interaction = _context.MovieInteractions.FirstOrDefault(x => x.Id == id);
            if (interaction != null)
            {
                _context.MovieInteractions.Remove(interaction);
                _context.SaveChanges();
            }
            return RedirectToAction("Profile");
        }


    }
}
