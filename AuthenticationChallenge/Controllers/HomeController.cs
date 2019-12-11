using AuthenticationChallenge.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AuthenticationChallenge.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Security PS's Intentionally Vulnerable Authentication Application";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Contact Security PS";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
