using Microsoft.AspNetCore.Mvc;
using Kinomatrix.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Kinomatrix.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }



    [HttpGet]
    public IActionResult LoginRegister()
    {
        return View();
    }




    [HttpPost]
    public IActionResult Register(string username, string password)
    {
        if (_context.Users.Any(u => u.Username == username))
        {
            return RedirectToAction("LoginRegister")
              .WithToast(this, "Username already exists");
        }

        string hash = ComputeHash(password);

        _context.Users.Add(new User { Username = username, PasswordHash = hash });
        _context.SaveChanges();

        return RedirectToAction("LoginRegister")
            .WithToast(this, "Registration successful! Please log in.");
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        string hash = ComputeHash(password);

        var user = _context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == hash);
        if (user == null)
        {
            return RedirectToAction("LoginRegister")
                 .WithToast(this, "Invalid login credentials!");
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        return RedirectToAction("Index", "Home")
           .WithToast(this, "Login successful!");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("UserId");
        return RedirectToAction("Login");
    }

    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}
