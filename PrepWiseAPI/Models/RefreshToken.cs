namespace PrepWiseAPI.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }

        // Computed property (not stored in DB)
        public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
    }
}
