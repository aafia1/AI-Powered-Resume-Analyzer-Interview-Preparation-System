using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.ViewModels;

namespace ResumeAnalyzer.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ─── Dashboard ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var resumes = await _db.Resumes.Include(r => r.User).Include(r => r.ResumeSkills).ThenInclude(rs => rs.Skill).ToListAsync();

            var skillFreq = resumes
                .SelectMany(r => r.ResumeSkills.Select(rs => rs.Skill!))
                .GroupBy(s => s.Name)
                .Select(g => new SkillFrequencyViewModel { SkillName = g.Key, Count = g.Count(), Category = g.First().Category })
                .OrderByDescending(s => s.Count)
                .Take(10)
                .ToList();

            // Score distribution
            var distribution = new List<ScoreDistributionViewModel>
            {
                new() { Range = "0-20", Count = resumes.Count(r => r.ResumeScore <= 20) },
                new() { Range = "21-40", Count = resumes.Count(r => r.ResumeScore is > 20 and <= 40) },
                new() { Range = "41-60", Count = resumes.Count(r => r.ResumeScore is > 40 and <= 60) },
                new() { Range = "61-80", Count = resumes.Count(r => r.ResumeScore is > 60 and <= 80) },
                new() { Range = "81-100", Count = resumes.Count(r => r.ResumeScore > 80) }
            };

            var userRoles = new List<UserManagementViewModel>();
            foreach (var u in users.Take(20))
            {
                var roles = await _userManager.GetRolesAsync(u);
                userRoles.Add(new UserManagementViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    ResumeCount = resumes.Count(r => r.UserId == u.Id),
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive,
                    Role = roles.FirstOrDefault() ?? "User"
                });
            }

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = users.Count,
                TotalResumes = resumes.Count,
                TotalAnalyses = await _db.AnalysisResults.CountAsync(),
                AvgResumeScore = resumes.Any() ? resumes.Average(r => r.ResumeScore) : 0,
                RecentUsers = userRoles.OrderByDescending(u => u.CreatedAt).Take(8).ToList(),
                TopSkills = skillFreq,
                ScoreDistribution = distribution,
                RecentResumes = resumes.OrderByDescending(r => r.UploadedAt).Take(10).Select(r => new ResumeAdminViewModel
                {
                    Id = r.Id,
                    UserName = r.User?.FullName ?? "Unknown",
                    FileName = r.FileName,
                    Score = r.ResumeScore,
                    UploadedAt = r.UploadedAt
                }).ToList()
            };

            return View(vm);
        }

        // ─── User Management ──────────────────────────────────────────────────
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var vmList = new List<UserManagementViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                vmList.Add(new UserManagementViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    ResumeCount = await _db.Resumes.CountAsync(r => r.UserId == u.Id),
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive,
                    Role = roles.FirstOrDefault() ?? "User"
                });
            }
            return View(vmList);
        }

        // ─── Toggle User Active ───────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(Users));
        }

        // ─── Delete Resume ────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResume(int id)
        {
            var resume = await _db.Resumes.FindAsync(id);
            if (resume == null) return NotFound();
            resume.IsActive = false;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Resume removed.";
            return RedirectToAction(nameof(Index));
        }

        // ─── Skills Overview ──────────────────────────────────────────────────
        public async Task<IActionResult> Skills()
        {
            var skills = await _db.Skills
                .Include(s => s.ResumeSkills)
                .OrderBy(s => s.Category).ThenBy(s => s.Name)
                .ToListAsync();
            return View(skills);
        }
    }
}
