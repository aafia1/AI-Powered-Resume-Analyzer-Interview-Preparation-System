using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.ViewModels;

namespace ResumeAnalyzer.Services
{
    /// <summary>
    /// Orchestrates full resume analysis and job-description matching.
    /// </summary>
    public class ResumeAnalysisService
    {
        private readonly ApplicationDbContext _db;
        private readonly ResumeParserService _parser;

        public ResumeAnalysisService(ApplicationDbContext db, ResumeParserService parser)
        {
            _db = db;
            _parser = parser;
        }

        // ─── Full analysis after upload ───────────────────────────────────────
        public async Task<ResumeAnalysisViewModel> AnalyzeResumeAsync(int resumeId)
        {
            var resume = await _db.Resumes
                .Include(r => r.ResumeSkills).ThenInclude(rs => rs.Skill)
                .FirstOrDefaultAsync(r => r.Id == resumeId)
                ?? throw new Exception("Resume not found");

            var text = resume.RawText ?? string.Empty;
            var skills = resume.ResumeSkills.Select(rs => rs.Skill!.Name).ToList();
            var questions = await GenerateInterviewQuestionsAsync(skills);
            var suggestions = _parser.GenerateSuggestions(text, skills);

            return new ResumeAnalysisViewModel
            {
                ResumeId = resume.Id,
                FileName = resume.FileName,
                ParsedName = resume.ParsedName,
                ParsedEmail = resume.ParsedEmail,
                ParsedPhone = resume.ParsedPhone,
                ResumeScore = resume.ResumeScore,
                AtsScore = resume.AtsScore,
                DetectedSkills = skills,
                Suggestions = suggestions,
                InterviewQuestions = questions,
                UploadedAt = resume.UploadedAt
            };
        }

        // ─── Job description matching ─────────────────────────────────────────
        public async Task<JobMatchViewModel> MatchWithJobAsync(int resumeId, string jobTitle, string jobDescription)
        {
            var resume = await _db.Resumes
                .Include(r => r.ResumeSkills).ThenInclude(rs => rs.Skill)
                .FirstOrDefaultAsync(r => r.Id == resumeId)
                ?? throw new Exception("Resume not found");

            var resumeSkills = resume.ResumeSkills.Select(rs => rs.Skill!.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var requiredSkills = _parser.DetectSkills(jobDescription);

            var matched = requiredSkills.Where(s => resumeSkills.Contains(s)).ToList();
            var missing = requiredSkills.Where(s => !resumeSkills.Contains(s)).ToList();

            int score = requiredSkills.Count == 0 ? 0
                : (int)Math.Round((double)matched.Count / requiredSkills.Count * 100);

            // Persist job description & analysis result
            var jd = new JobDescription
            {
                UserId = resume.UserId,
                JobTitle = jobTitle,
                Description = jobDescription
            };
            _db.JobDescriptions.Add(jd);

            var result = new AnalysisResult
            {
                ResumeId = resumeId,
                ResumeScore = resume.ResumeScore,
                AtsScore = resume.AtsScore,
                JobMatchScore = score,
                MatchedSkills = JsonConvert.SerializeObject(matched),
                MissingSkills = JsonConvert.SerializeObject(missing),
                Suggestions = JsonConvert.SerializeObject(BuildJobRecommendations(missing))
            };
            _db.AnalysisResults.Add(result);
            await _db.SaveChangesAsync();

            var recommendations = BuildJobRecommendations(missing);

            return new JobMatchViewModel
            {
                ResumeId = resumeId,
                ResumeFileName = resume.FileName,
                JobTitle = jobTitle,
                JobDescription = jobDescription,
                MatchedSkills = matched,
                MissingSkills = missing,
                CompatibilityScore = score,
                Recommendations = recommendations,
                IsAnalyzed = true
            };
        }

        // ─── Interview questions ──────────────────────────────────────────────
        public async Task<List<InterviewQuestionViewModel>> GenerateInterviewQuestionsAsync(List<string> skills)
        {
            if (!skills.Any()) return new List<InterviewQuestionViewModel>();

            var skillNames = skills.Select(s => s.ToLower()).ToList();
            var questions = await _db.InterviewQuestions
                .Where(q => skillNames.Contains(q.SkillName.ToLower()))
                .Take(15)
                .ToListAsync();

            return questions.Select(q => new InterviewQuestionViewModel
            {
                Skill = q.SkillName,
                Question = q.Question,
                Level = q.Level,
                Category = q.Category,
                LevelColor = q.Level switch
                {
                    "Beginner" => "success",
                    "Intermediate" => "warning",
                    "Advanced" => "danger",
                    _ => "secondary"
                }
            }).ToList();
        }

        // ─── Private helpers ──────────────────────────────────────────────────
        private static List<SuggestionViewModel> BuildJobRecommendations(List<string> missingSkills)
        {
            var recs = new List<SuggestionViewModel>();
            if (missingSkills.Any())
                recs.Add(new SuggestionViewModel
                {
                    Icon = "fas fa-graduation-cap",
                    Title = "Learn Missing Skills",
                    Description = $"Consider learning: {string.Join(", ", missingSkills.Take(5))} to improve your match rate.",
                    Priority = "High",
                    BadgeColor = "danger"
                });

            recs.Add(new SuggestionViewModel
            {
                Icon = "fas fa-tags",
                Title = "Tailor Your Resume",
                Description = "Customize your resume for each job application using keywords from the job description.",
                Priority = "High",
                BadgeColor = "danger"
            });

            recs.Add(new SuggestionViewModel
            {
                Icon = "fas fa-project-diagram",
                Title = "Highlight Relevant Projects",
                Description = "Move projects that use required skills to the top of your projects section.",
                Priority = "Medium",
                BadgeColor = "warning"
            });

            return recs;
        }

        // ─── Dashboard stats ──────────────────────────────────────────────────
        public async Task<DashboardViewModel> GetDashboardAsync(string userId)
        {
            var resumes = await _db.Resumes
                .Where(r => r.UserId == userId && r.IsActive)
                .Include(r => r.ResumeSkills).ThenInclude(rs => rs.Skill)
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();

            var skillFreq = resumes
                .SelectMany(r => r.ResumeSkills.Select(rs => rs.Skill!))
                .GroupBy(s => s.Name)
                .Select(g => new SkillFrequencyViewModel { SkillName = g.Key, Count = g.Count(), Category = g.First().Category })
                .OrderByDescending(s => s.Count)
                .Take(8)
                .ToList();

            var cards = resumes.Take(6).Select(r => new ResumeCardViewModel
            {
                Id = r.Id,
                FileName = r.FileName,
                Score = r.ResumeScore,
                AtsScore = r.AtsScore,
                UploadedAt = r.UploadedAt,
                Skills = r.ResumeSkills.Select(rs => rs.Skill!.Name).Take(5).ToList()
            }).ToList();

            return new DashboardViewModel
            {
                TotalResumes = resumes.Count,
                AvgResumeScore = resumes.Any() ? (int)resumes.Average(r => r.ResumeScore) : 0,
                AvgAtsScore = resumes.Any() ? (int)resumes.Average(r => r.AtsScore) : 0,
                TotalSkills = resumes.SelectMany(r => r.ResumeSkills).Select(rs => rs.SkillId).Distinct().Count(),
                RecentResumes = cards,
                TopSkills = skillFreq,
                ScoreTrend = resumes.Take(7).Reverse().Select(r => r.ResumeScore).ToList(),
                ScoreTrendLabels = resumes.Take(7).Reverse().Select(r => r.UploadedAt.ToString("MMM dd")).ToList()
            };
        }
    }
}
