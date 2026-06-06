using System.Text.Json;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.EntityFrameworkCore;
using PrepWiseAPI.Data;
using PrepWiseAPI.DTOs;
using PrepWiseAPI.Helpers;
using PrepWiseAPI.Models;

namespace PrepWiseAPI.Services
{
    public class ResumeService
    {
        private readonly AppDbContext _context;
        private readonly GeminiService _geminiService;
        private readonly IWebHostEnvironment _env;

        public ResumeService(AppDbContext context, GeminiService geminiService, IWebHostEnvironment env)
        {
            _context = context;
            _geminiService = geminiService;
            _env = env;
        }

        public async Task<ResumeReportDto> UploadAndAnalyzeAsync(int userId, IFormFile file, string targetRole)
        {
            // 1. Validate file
            if (file.Length == 0 || !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Please upload a valid PDF file.");

            if (file.Length > 5 * 1024 * 1024) // 5MB limit
                throw new Exception("File size must be under 5MB.");

            // 2. Save file to disk
            var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads", "Resumes");
            Directory.CreateDirectory(uploadsDir);

            var uniqueFileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{file.FileName}";
            var filePath = Path.Combine(uploadsDir, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 3. Extract text from PDF
            var resumeText = ExtractTextFromPdf(filePath);

            if (string.IsNullOrWhiteSpace(resumeText) || resumeText.Length < 50)
                throw new Exception("Could not extract enough text from the PDF. Make sure it's not an image-only resume.");

            // 4. Save resume record
            var resume = new Resume
            {
                UserId = userId,
                FileName = file.FileName,
                FilePath = filePath,
                TargetRole = targetRole
            };

            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();

            // 5. Call Gemini AI for analysis
            var prompt = PromptTemplates.ResumeAnalysis(resumeText, targetRole);
            var aiResponse = await _geminiService.GenerateContentAsync(prompt);

            // 6. Parse AI response
            // Clean up response — remove markdown code blocks if present
            aiResponse = aiResponse.Trim();
            if (aiResponse.StartsWith("```json")) aiResponse = aiResponse[7..];
            if (aiResponse.StartsWith("```")) aiResponse = aiResponse[3..];
            if (aiResponse.EndsWith("```")) aiResponse = aiResponse[..^3];
            aiResponse = aiResponse.Trim();

            var analysisResult = JsonSerializer.Deserialize<JsonElement>(aiResponse);

            var report = new ResumeReport
            {
                ResumeId = resume.Id,
                Score = analysisResult.GetProperty("score").GetInt32(),
                Summary = analysisResult.GetProperty("summary").GetString() ?? "",
                Strengths = JsonSerializer.Serialize(GetStringArray(analysisResult, "strengths")),
                ImprovementAreas = JsonSerializer.Serialize(GetStringArray(analysisResult, "improvementAreas")),
                MissingSkills = JsonSerializer.Serialize(GetStringArray(analysisResult, "missingSkills"))
            };

            _context.ResumeReports.Add(report);
            await _context.SaveChangesAsync();

            // 7. Return DTO
            return MapToDto(resume, report);
        }

        public async Task<List<ResumeReportDto>> GetUserReportsAsync(int userId)
        {
            var resumes = await _context.Resumes
                .Where(r => r.UserId == userId)
                .Include(r => r.Report)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return resumes
                .Where(r => r.Report != null)
                .Select(r => MapToDto(r, r.Report!))
                .ToList();
        }

        public async Task<ResumeReportDto?> GetReportByIdAsync(int userId, int reportId)
        {
            var report = await _context.ResumeReports
                .Include(r => r.Resume)
                .FirstOrDefaultAsync(r => r.Id == reportId && r.Resume.UserId == userId);

            if (report == null) return null;

            return MapToDto(report.Resume, report);
        }

        // ===== Private Helpers =====

        private string ExtractTextFromPdf(string filePath)
        {
            var text = new System.Text.StringBuilder();

            using (var reader = new PdfReader(filePath))
            using (var pdfDoc = new PdfDocument(reader))
            {
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var page = pdfDoc.GetPage(i);
                    var strategy = new SimpleTextExtractionStrategy();
                    var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                    text.AppendLine(pageText);
                }
            }

            return text.ToString().Trim();
        }

        private List<string> GetStringArray(JsonElement element, string propertyName)
        {
            var result = new List<string>();
            if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in prop.EnumerateArray())
                {
                    var value = item.GetString();
                    if (!string.IsNullOrEmpty(value)) result.Add(value);
                }
            }
            return result;
        }

        private ResumeReportDto MapToDto(Resume resume, ResumeReport report)
        {
            return new ResumeReportDto
            {
                Id = report.Id,
                ResumeId = resume.Id,
                FileName = resume.FileName,
                TargetRole = resume.TargetRole,
                Score = report.Score,
                Summary = report.Summary,
                Strengths = JsonSerializer.Deserialize<List<string>>(report.Strengths) ?? new(),
                ImprovementAreas = JsonSerializer.Deserialize<List<string>>(report.ImprovementAreas) ?? new(),
                MissingSkills = JsonSerializer.Deserialize<List<string>>(report.MissingSkills) ?? new(),
                CreatedAt = resume.CreatedAt
            };
        }
    }
}
