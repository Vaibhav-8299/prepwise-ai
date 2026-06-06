namespace PrepWiseAPI.Models
{
    public class ResumeReport
    {
        public int Id { get; set; }
        public int ResumeId { get; set; }
        public int Score { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Strengths { get; set; } = "[]";           // JSON array
        public string ImprovementAreas { get; set; } = "[]";    // JSON array
        public string MissingSkills { get; set; } = "[]";       // JSON array
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Resume Resume { get; set; } = null!;
    }
}
