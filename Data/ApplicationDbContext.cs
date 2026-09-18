using CommunityComplaintApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaintApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Complaint> Complaints { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasIndex(u => u.MobileNumber).IsUnique();
            });

            modelBuilder.Entity<Complaint>(entity =>
            {
                entity.ToTable("Complaints");
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Complaints)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("Admins");
                entity.HasIndex(a => a.Username).IsUnique();
            });
        }
    }
}
