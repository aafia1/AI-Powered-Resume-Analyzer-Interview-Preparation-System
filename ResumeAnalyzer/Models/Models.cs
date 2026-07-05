using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ResumeAnalyzer.Models
{
    // ─── ApplicationUser ─────────────────────────────────────────────────────
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ProfilePicture { get; set; }
        public string? LinkedInUrl { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    }

    // ─── Resume ───────────────────────────────────────────────────────────────
    public class Resume
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required, StringLength(200)]
        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty; // pdf/docx

        // Parsed fields
        public string? ParsedName { get; set; }
        public string? ParsedEmail { get; set; }
        public string? ParsedPhone { get; set; }
        public string? ParsedSummary { get; set; }
        public string? RawText { get; set; }

        public int ResumeScore { get; set; }
        public int AtsScore { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public ICollection<ResumeSkill> ResumeSkills { get; set; } = new List<ResumeSkill>();
        public ICollection<AnalysisResult> AnalysisResults { get; set; } = new List<AnalysisResult>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }

    // ─── Skill ────────────────────────────────────────────────────────────────
    public class Skill
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;   // Frontend/Backend/etc
        public string Proficiency { get; set; } = string.Empty; // Beginner/Intermediate/Advanced

        public ICollection<ResumeSkill> ResumeSkills { get; set; } = new List<ResumeSkill>();
    }

    // ─── ResumeSkill (junction) ───────────────────────────────────────────────
    public class ResumeSkill
    {
        public int ResumeId { get; set; }
        public Resume? Resume { get; set; }

        public int SkillId { get; set; }
        public Skill? Skill { get; set; }
    }

    // ─── JobDescription ───────────────────────────────────────────────────────
    public class JobDescription
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required, StringLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AnalysisResult> AnalysisResults { get; set; } = new List<AnalysisResult>();
    }

    // ─── AnalysisResult ───────────────────────────────────────────────────────
    public class AnalysisResult
    {
        public int Id { get; set; }

        public int ResumeId { get; set; }
        public Resume? Resume { get; set; }

        public int? JobDescriptionId { get; set; }
        public JobDescription? JobDescription { get; set; }

        public int ResumeScore { get; set; }
        public int AtsScore { get; set; }
        public int JobMatchScore { get; set; }

        public string MatchedSkills { get; set; } = string.Empty;   // JSON
        public string MissingSkills { get; set; } = string.Empty;   // JSON
        public string Suggestions { get; set; } = string.Empty;     // JSON

        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    // ─── InterviewQuestion ────────────────────────────────────────────────────
    public class InterviewQuestion
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string SkillName { get; set; } = string.Empty;

        [Required]
        public string Question { get; set; } = string.Empty;

        public string Level { get; set; } = "Beginner"; // Beginner/Intermediate/Advanced
        public string Category { get; set; } = string.Empty;
    }

    // ─── Report ───────────────────────────────────────────────────────────────
    public class Report
    {
        public int Id { get; set; }

        public int ResumeId { get; set; }
        public Resume? Resume { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string ReportPath { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string ReportType { get; set; } = "Full"; // Full/Summary
    }
}
