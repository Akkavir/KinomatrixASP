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

        public IActionResult Profile(string sortBy = "DateTime", string sortOrder = "desc")
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var interactions = _context.MovieInteractions
                .Where(x => x.UserId == userId);

            interactions = (sortBy, sortOrder.ToLower()) switch
            {
                ("MovieId", "asc") => interactions.OrderBy(x => x.MovieId),
                ("MovieId", "desc") => interactions.OrderByDescending(x => x.MovieId),
                ("Rating", "asc") => interactions.OrderBy(x => x.Rating),
                ("Rating", "desc") => interactions.OrderByDescending(x => x.Rating),
                ("InWatchlist", "asc") => interactions.OrderBy(x => x.InWatchlist),
                ("InWatchlist", "desc") => interactions.OrderByDescending(x => x.InWatchlist),
                ("DateTime", "asc") => interactions.OrderBy(x => x.DateTime),
                _ => interactions.OrderByDescending(x => x.DateTime)
            };

            return View(interactions.ToList());
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
