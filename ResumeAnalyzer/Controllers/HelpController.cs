using Microsoft.AspNetCore.Mvc;

namespace ResumeAnalyzer.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult Documentation()
        {
            return View();
        }

        public IActionResult Support()
        {
            return View();
        }

        public IActionResult UserGuide()
        {
            return View();
        }
    }
}
