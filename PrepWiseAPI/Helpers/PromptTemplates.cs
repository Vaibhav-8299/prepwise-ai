namespace PrepWiseAPI.Helpers
{
    public static class PromptTemplates
    {
        public static string ResumeAnalysis(string resumeText, string targetRole)
        {
            return $@"You are an expert resume reviewer and career advisor.

Analyze the following resume for the target role: {targetRole}

Resume Content:
{resumeText}

Respond ONLY with valid JSON in this exact format (no markdown, no code blocks, no extra text):
{{
  ""score"": <number between 0-100>,
  ""summary"": ""<2-3 sentence overall assessment>"",
  ""strengths"": [""<strength 1>"", ""<strength 2>"", ""<strength 3>""],
  ""improvementAreas"": [""<area 1>"", ""<area 2>"", ""<area 3>""],
  ""missingSkills"": [""<skill 1>"", ""<skill 2>"", ""<skill 3>""]
}}

Scoring guidelines:
- 80-100: Excellent match for the role
- 60-79: Good match with minor gaps
- 40-59: Moderate match, needs improvement
- 0-39: Significant gaps for this role

Be specific and actionable in your feedback. Reference actual content from the resume.";
        }
    }
}
