using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.Services;

namespace ResumeAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        public IActionResult About() => View();
        public IActionResult Features() => View();
        public IActionResult Privacy() => View();
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ResumeAnalysisService _analysisService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ResumeAnalysisService analysisService, UserManager<ApplicationUser> userManager)
        {
            _analysisService = analysisService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User)
                ?? throw new Exception("User not found");
            var vm = await _analysisService.GetDashboardAsync(user.Id);
            vm.UserName = user.FullName;
            return View(vm);
        }
    }
}
