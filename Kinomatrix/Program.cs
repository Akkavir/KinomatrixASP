using Microsoft.EntityFrameworkCore;
using System;

namespace Kinomatrix
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHttpClient();
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            // Add services to the container.
            builder.Services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/Account/LoginRegister";
                    options.LogoutPath = "/Account/Logout";
                });

            builder.Services.AddControllersWithViews();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1); // lub inny czas
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();




            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseDefaultFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            app.Run();
        }
    }
}
