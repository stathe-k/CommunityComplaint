using CommunityComplaintApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaintApp.Data
{
    // Applies pending migrations and seeds a default admin account on first run.
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

            if (!context.Admins.Any())
            {
                var admin = new Admin { Username = "admin" };
                var hasher = new PasswordHasher<Admin>();
                // Default credentials for first login — change immediately after setup.
                admin.Password = hasher.HashPassword(admin, "Admin@123");

                context.Admins.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
