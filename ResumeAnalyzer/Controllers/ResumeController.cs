using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.Services;
using ResumeAnalyzer.ViewModels;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    public class ResumeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ResumeParserService _parser;
        private readonly ResumeAnalysisService _analysisService;
        private readonly PdfReportService _reportService;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedExtensions = { ".pdf", ".docx" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public ResumeController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            ResumeParserService parser,
            ResumeAnalysisService analysisService,
            PdfReportService reportService,
            IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _parser = parser;
            _analysisService = analysisService;
            _reportService = reportService;
            _env = env;
        }

        // ─── Upload GET ───────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Upload() => View();

        // ─── Upload POST ──────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile resumeFile, string? jobTitle = null)
        {
            if (resumeFile == null || resumeFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a file.");
                return View();
            }

            var ext = Path.GetExtension(resumeFile.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                ModelState.AddModelError(string.Empty, "Only PDF and DOCX files are allowed.");
                return View();
            }

            if (resumeFile.Length > MaxFileSizeBytes)
            {
                ModelState.AddModelError(string.Empty, "File size must be under 10 MB.");
                return View();
            }

            var user = await _userManager.GetUserAsync(User)!;

            // Save file
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "resumes");
            Directory.CreateDirectory(uploadDir);
            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadDir, uniqueName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                await resumeFile.CopyToAsync(stream);

            // Parse
            var rawText = await _parser.ExtractTextAsync(filePath, ext.TrimStart('.'));
            var skills = _parser.DetectSkills(rawText);
            var resumeScore = _parser.CalculateResumeScore(rawText, skills);
            var atsScore = _parser.CalculateAtsScore(rawText, skills);

            // Save resume record
            var resume = new Resume
            {
                UserId = user!.Id,
                FileName = resumeFile.FileName,
                FilePath = $"/uploads/resumes/{uniqueName}",
                FileSize = resumeFile.Length,
                FileType = ext.TrimStart('.'),
                RawText = rawText.Length > 10000 ? rawText[..10000] : rawText,
                ParsedName = _parser.ExtractName(rawText),
                ParsedEmail = _parser.ExtractEmail(rawText),
                ParsedPhone = _parser.ExtractPhone(rawText),
                ResumeScore = resumeScore,
                AtsScore = atsScore
            };
            _db.Resumes.Add(resume);
            await _db.SaveChangesAsync();

            // Link skills
            foreach (var skillName in skills)
            {
                var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Name.ToLower() == skillName.ToLower());
                if (skill == null)
                {
                    skill = new Skill { Name = skillName, Category = "Other", Proficiency = "Unknown" };
                    _db.Skills.Add(skill);
                    await _db.SaveChangesAsync();
                }
                if (!_db.ResumeSkills.Any(rs => rs.ResumeId == resume.Id && rs.SkillId == skill.Id))
                    _db.ResumeSkills.Add(new ResumeSkill { ResumeId = resume.Id, SkillId = skill.Id });
            }
            await _db.SaveChangesAsync();

            TempData["Success"] = "Resume uploaded and analyzed successfully!";
            return RedirectToAction(nameof(Analysis), new { id = resume.Id });
        }

        // ─── Analysis ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Analysis(int id)
        {
            var user = await _userManager.GetUserAsync(User)!;
            var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == user!.Id);
            if (resume == null) return NotFound();

            var vm = await _analysisService.AnalyzeResumeAsync(id);
            return View(vm);
        }

        // ─── Job Match GET ────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> JobMatch(int? resumeId)
        {
            var user = await _userManager.GetUserAsync(User)!;
            var resumes = await _db.Resumes
                .Where(r => r.UserId == user!.Id && r.IsActive)
                .Select(r => new { r.Id, r.FileName })
                .ToListAsync();

            ViewBag.Resumes = resumes;
            ViewBag.SelectedResumeId = resumeId ?? 0;
            return View(new JobMatchViewModel());
        }

        // ─── Job Match POST ───────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> JobMatch(int resumeId, string jobTitle, string jobDescription)
        {
            var user = await _userManager.GetUserAsync(User)!;
            var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == user!.Id);
            if (resume == null) return NotFound();

            var vm = await _analysisService.MatchWithJobAsync(resumeId, jobTitle, jobDescription);

            var resumes = await _db.Resumes
                .Where(r => r.UserId == user!.Id && r.IsActive)
                .Select(r => new { r.Id, r.FileName })
                .ToListAsync();
            ViewBag.Resumes = resumes;
            ViewBag.SelectedResumeId = resumeId;

            return View(vm);
        }

        // ─── History ──────────────────────────────────────────────────────────
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User)!;
            var resumes = await _db.Resumes
                .Where(r => r.UserId == user!.Id && r.IsActive)
                .Include(r => r.ResumeSkills).ThenInclude(rs => rs.Skill)
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();

            var vm = new ResumeHistoryViewModel
            {
                TotalCount = resumes.Count,
                AverageScore = resumes.Any() ? resumes.Average(r => r.ResumeScore) : 0,
                BestScore = resumes.Any() ? resumes.Max(r => r.ResumeScore) : 0,
                Resumes = resumes.Select(r => new ResumeCardViewModel
                {
                    Id = r.Id,
                    FileName = r.FileName,
                    Score = r.ResumeScore,
                    AtsScore = r.AtsScore,
                    UploadedAt = r.UploadedAt,
                    Skills = r.ResumeSkills.Select(rs => rs.Skill!.Name).Take(5).ToList()
                }).ToList()
            };

            return View(vm);
        }

        // ─── Delete ───────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User)!;
            var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == user!.Id);
            if (resume == null) return NotFound();

            resume.IsActive = false;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Resume deleted.";
            return RedirectToAction(nameof(History));
        }

        // ─── Generate Report ──────────────────────────────────────────────────
        public async Task<IActionResult> GenerateReport(int id)
        {
            var user = await _userManager.GetUserAsync(User)!;
            var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == user!.Id);
            if (resume == null) return NotFound();

            var analysis = await _analysisService.AnalyzeResumeAsync(id);
            var path = _reportService.GenerateReport(analysis, user!.FullName);

            _db.Reports.Add(new Report { ResumeId = id, UserId = user.Id, ReportPath = path });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Report generated!";
            return Redirect(path);
        }

        // ─── Interview Questions ───────────────────────────────────────────────
        public async Task<IActionResult> InterviewQuestions(int id)
        {
            var user = await _userManager.GetUserAsync(User)!;
            var resume = await _db.Resumes
                .Include(r => r.ResumeSkills).ThenInclude(rs => rs.Skill)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user!.Id);
            if (resume == null) return NotFound();

            var skills = resume.ResumeSkills.Select(rs => rs.Skill!.Name).ToList();
            var questions = await _analysisService.GenerateInterviewQuestionsAsync(skills);

            ViewBag.ResumeId = id;
            ViewBag.FileName = resume.FileName;
            ViewBag.Skills = skills;
            return View(questions);
        }
    }
}
