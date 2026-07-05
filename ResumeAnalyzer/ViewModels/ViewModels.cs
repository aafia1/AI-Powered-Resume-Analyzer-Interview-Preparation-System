using System.ComponentModel.DataAnnotations;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.ViewModels
{
    // ─── Auth ViewModels ──────────────────────────────────────────────────────
    public class RegisterViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // ─── Dashboard ViewModel ──────────────────────────────────────────────────
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public int TotalResumes { get; set; }
        public int AvgResumeScore { get; set; }
        public int AvgAtsScore { get; set; }
        public int TotalSkills { get; set; }
        public List<ResumeCardViewModel> RecentResumes { get; set; } = new();
        public List<SkillFrequencyViewModel> TopSkills { get; set; } = new();
        public List<int> ScoreTrend { get; set; } = new();
        public List<string> ScoreTrendLabels { get; set; } = new();
    }

    public class ResumeCardViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int AtsScore { get; set; }
        public DateTime UploadedAt { get; set; }
        public List<string> Skills { get; set; } = new();
    }

    public class SkillFrequencyViewModel
    {
        public string SkillName { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    // ─── Resume ViewModels ────────────────────────────────────────────────────
    public class ResumeAnalysisViewModel
    {
        public int ResumeId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? ParsedName { get; set; }
        public string? ParsedEmail { get; set; }
        public string? ParsedPhone { get; set; }
        public int ResumeScore { get; set; }
        public int AtsScore { get; set; }
        public List<string> DetectedSkills { get; set; } = new();
        public List<SuggestionViewModel> Suggestions { get; set; } = new();
        public List<InterviewQuestionViewModel> InterviewQuestions { get; set; } = new();
        public DateTime UploadedAt { get; set; }
    }

    public class JobMatchViewModel
    {
        public int ResumeId { get; set; }
        public string ResumeFileName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public int CompatibilityScore { get; set; }
        public List<SuggestionViewModel> Recommendations { get; set; } = new();
        public bool IsAnalyzed { get; set; } = false;
    }

    public class SuggestionViewModel
    {
        public string Icon { get; set; } = "fas fa-lightbulb";
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium"; // High/Medium/Low
        public string BadgeColor { get; set; } = "warning";
    }

    public class InterviewQuestionViewModel
    {
        public string Skill { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string LevelColor { get; set; } = "success";
    }

    // ─── Admin ViewModel ─────────────────────────────────────────────────────
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalResumes { get; set; }
        public int TotalAnalyses { get; set; }
        public double AvgResumeScore { get; set; }
        public List<UserManagementViewModel> RecentUsers { get; set; } = new();
        public List<SkillFrequencyViewModel> TopSkills { get; set; } = new();
        public List<ScoreDistributionViewModel> ScoreDistribution { get; set; } = new();
        public List<ResumeAdminViewModel> RecentResumes { get; set; } = new();
    }

    public class UserManagementViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ResumeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class ScoreDistributionViewModel
    {
        public string Range { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ResumeAdminViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int Score { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    // ─── History ViewModel ────────────────────────────────────────────────────
    public class ResumeHistoryViewModel
    {
        public List<ResumeCardViewModel> Resumes { get; set; } = new();
        public int TotalCount { get; set; }
        public double AverageScore { get; set; }
        public int BestScore { get; set; }
    }
}
