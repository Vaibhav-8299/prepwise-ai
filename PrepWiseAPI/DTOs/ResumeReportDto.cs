namespace PrepWiseAPI.DTOs
{
    public class ResumeReportDto
    {
        public int Id { get; set; }
        public int ResumeId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string TargetRole { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = new();
        public List<string> ImprovementAreas { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
