using Microsoft.EntityFrameworkCore;
using PrepWiseAPI.Models;

namespace PrepWiseAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<OtpRecord> OtpRecords { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Users Table =====
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(u => u.Role).IsRequired().HasMaxLength(20).HasDefaultValue("Student");
                entity.Property(u => u.IsEmailVerified).HasDefaultValue(false);
                entity.Property(u => u.IsBlocked).HasDefaultValue(false);
            });

            // ===== OtpRecords Table =====
            modelBuilder.Entity<OtpRecord>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.OtpCode).IsRequired().HasMaxLength(6);
                entity.Property(o => o.Purpose).IsRequired().HasMaxLength(20);

                entity.HasOne(o => o.User)
                      .WithMany(u => u.OtpRecords)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== RefreshTokens Table =====
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Token).IsRequired().HasMaxLength(255);
                entity.HasIndex(r => r.Token).IsUnique();
                entity.Property(r => r.RevokedAt).IsRequired(false);

                // Ignore computed property — EF should not map it to a column
                entity.Ignore(r => r.IsActive);

                entity.HasOne(r => r.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
