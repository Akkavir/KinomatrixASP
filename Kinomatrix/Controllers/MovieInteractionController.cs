using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class MovieInteractionController : ControllerBase
{
    private readonly AppDbContext _db;

    public MovieInteractionController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("watchlist")]
    public IActionResult AddToWatchlist([FromBody] MovieInteraction interaction)
    {
        _db.MovieInteractions.Add(interaction);
        _db.SaveChanges();
        return Ok();
    }

    [HttpPost("rate")]
    public IActionResult RateMovie([FromBody] MovieInteraction interaction)
    {
        _db.MovieInteractions.Add(interaction);
        _db.SaveChanges();
        return Ok();
    }

    [HttpGet("user/{userId}")]
    public IActionResult GetUserInteractions(int userId)
    {
        var interactions = _db.MovieInteractions.Where(m => m.UserId == userId).ToList();
        return Ok(interactions);
    }
}
