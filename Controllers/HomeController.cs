using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SHL_Platform.Data;

namespace SHL_Platform.Controllers
{
    public class HomeController(ApplicationContext context) : Controller
    {
        private readonly ApplicationContext _context = context;

        public IActionResult Index()
        {
            var nonAdminUsers = _context.Users
                .Where(u => !u.IsAdmin)
                .Include(u => u.Country) // Include the Country navigation property
                .ToList();
            return View(nonAdminUsers);
        }

        [HttpPost]
        public IActionResult DeleteUser(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult UserPage()
        {

            var admin = _context.Admins.FirstOrDefault(u => u.Username == User.Identity.Name);
            string layoutPath = admin != null ? "_Layout" : "_UserLayout";
            ViewBag.LayoutPath = layoutPath;
            var surveys = _context.Surveys
                .Where(u => u.CreatedBy == User.Identity.Name).ToList();
            return View(surveys);
        }
        public IActionResult ViewSurvey(Guid token)
        {
            if (token != null)
            {
                var record = _context.Surveys.FirstOrDefault(e => e.UniqueId == token);
                if (record != null)
                {
                    ViewBag.Token = token;
                    var admin = _context.Admins.FirstOrDefault(u => u.Username == User.Identity.Name);
                    string layoutPath = admin != null ? "_Layout" : "_UserLayout";
                    ViewBag.LayoutPath = layoutPath;
                    return View(record);
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public IActionResult DeleteSurvey(int surveyId)
        {
            var survey = _context.Surveys.Find(surveyId);
            if (survey != null)
            {
                _context.Surveys.Remove(survey);
                _context.SaveChanges();
            }
            return RedirectToAction("UserPage");
        }

        [HttpGet]
        public IActionResult DownloadFile(string fileName, string key, string token)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments", token, key, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(); // Or handle the case where the file doesn't exist
            }

            var fileContent = System.IO.File.ReadAllBytes(filePath);
            return File(fileContent, "application/octet-stream", fileName);
        }
    }
}