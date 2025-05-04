using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] User user)
    {
        if (_db.Users.Any(u => u.Username == user.Username))
            return BadRequest("User already exists.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        _db.Users.Add(user);
        _db.SaveChanges();
        return Ok("Registered successfully.");
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] User login)
    {
        var user = _db.Users.SingleOrDefault(u => u.Username == login.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(login.PasswordHash, user.PasswordHash))
            return Unauthorized("Invalid login.");

        return Ok(new { user.Id, user.Username });
    }
}
