using Microsoft.EntityFrameworkCore;
using System.Text.Json;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<MovieInteraction> MovieInteractions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }

}
