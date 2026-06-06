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
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<ResumeReport> ResumeReports { get; set; }

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

            // ===== Resumes Table =====
            modelBuilder.Entity<Resume>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.FileName).IsRequired().HasMaxLength(255);
                entity.Property(r => r.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(r => r.TargetRole).IsRequired().HasMaxLength(100);

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== ResumeReports Table =====
            modelBuilder.Entity<ResumeReport>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Summary).IsRequired().HasMaxLength(2000);
                entity.Property(r => r.Strengths).IsRequired().HasMaxLength(2000);
                entity.Property(r => r.ImprovementAreas).IsRequired().HasMaxLength(2000);
                entity.Property(r => r.MissingSkills).IsRequired().HasMaxLength(2000);

                entity.HasOne(r => r.Resume)
                      .WithOne(res => res.Report)
                      .HasForeignKey<ResumeReport>(r => r.ResumeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
