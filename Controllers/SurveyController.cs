using Microsoft.AspNetCore.Mvc;
using SHL_Platform.Data;
using SHL_Platform.Models;
using Newtonsoft.Json;
using SHL_Platform.Services;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net.Mime;
using SHL_Platform.ViewModels;

namespace SHL_Platform.Controllers
{
    public class SurveyController(ApplicationContext context, IWebHostEnvironment hostingEnvironment) : Controller
    {
        private readonly ApplicationContext _context = context;
        private readonly IWebHostEnvironment _hostingEnvironment = hostingEnvironment;



        public IActionResult NewSurvey()
        {
            var admin = _context.Admins.FirstOrDefault(u => u.Username == User.Identity.Name);
            string layoutPath = admin != null ? "_Layout" : "_UserLayout";
            ViewBag.LayoutPath = layoutPath;
            return View();
        }

        [HttpPost]
        public IActionResult NewSurvey([FromBody] string stringifyData)
        {

            if (!string.IsNullOrEmpty(stringifyData))
            {
                Guid uniqueId = Guid.NewGuid();

                Survey survey = new()
                {
                    FormDataJson = stringifyData,
                    UniqueId = uniqueId,
                    CreatedBy = User.Identity.Name
                };
                _context.Surveys.Add(survey);
                _context.SaveChanges();
                string redirectUrl = Url.Action("SendSurvey", "Survey", new { uniqueId });
                return Ok(new { success = true, uniqueId, redirectUrl });
            }
            // Return a JSON response indicating failure
            return BadRequest(new { success = false });
        }

        public IActionResult SendSurvey(Guid uniqueId)
        {
            var admin = _context.Admins.FirstOrDefault(u => u.Username == User.Identity.Name);
            string layoutPath = admin != null ? "_Layout" : "_UserLayout";
            ViewBag.LayoutPath = layoutPath;
            ViewBag.UniqueId = uniqueId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendSurvey(Guid uniqueId, string email, string bcc, string cc, string surveyTag, [FromServices] EmailService emailservices)
        {
            var record = _context.Surveys.FirstOrDefault(e => e.UniqueId == uniqueId);
            if (record == null)
            {
                return NotFound(); // Handle if the record with the given uniqueId is not found
            }

            record.SendToEmail = email;
            record.Bcc = bcc;
            record.Cc = cc;
            record.SurveyTags = surveyTag;

            // Save changes to the database
            _context.SaveChanges();

            // Prepare link URL for the survey
            string baseUrl = "https://localhost:7218";
            string linkUrl = $"{baseUrl}/Survey/ShowSurvey?token={uniqueId}";

            // Get the uploaded logo file
            var logoFile = Request.Form.Files.GetFile("logo");
            if (logoFile != null && logoFile.Length > 0)
            {
                // Create a unique folder for the logo using the uniqueId
                string uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images", uniqueId.ToString());
                Directory.CreateDirectory(uploadFolder); // Create directory if it doesn't exist

                // Construct the path for saving the logo
                string logoPath = Path.Combine(uploadFolder, logoFile.FileName);

                // Save the logo file to the specified path
                using (var fileStream = new FileStream(logoPath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(fileStream);
                }

                // Create an attachment for the logo
                Attachment logoAttachment = new Attachment(logoPath);

                // Load the logo image from the server
                string logoImagePath = $"/images/{uniqueId}/{logoFile.FileName}";

                // Modify the email body to include the logo
                string emailBody = $"Your Survey Link: <a href='{linkUrl}'>Survey Link</a>";
                string linkedLogo = $"<img src='{logoImagePath}' alt='Logo'>";
                emailBody += $"<br/><br/>{linkedLogo}";

                // Send the survey email with the logo attachment
                await emailservices.SendEmailAsync(email, "Your New Survey", emailBody, logoAttachment, bcc, cc);
            }
            else
            {
                // If no logo was uploaded, send the email without an attachment
                await emailservices.SendEmailAsync(email, "Your New Survey", $"Your Survey Link: <a href='{linkUrl}'>Survey Link</a>", null, bcc, cc);
            }

            // Redirect to a page after survey sent
            return RedirectToAction("UserPage", "Home");
        }

        [HttpGet]
        public IActionResult ShowSurvey(Guid token)
        {
            if (token != null)
            {
                var record = _context.Surveys.FirstOrDefault(e => e.UniqueId == token);
                if (record != null)
                {
                    if (record.UpdatedSurvey != null)
                    {
                        string script = @"alert('Your Survey has been already completed');window.close();";
                        return Content("<script>" + script + "</script>", "text/html");
                    }
                    ViewBag.Token = token;
                    return View(record);
                }
            }
            return BadRequest();

        }
        [HttpPost]
        public IActionResult UpdateSurvey([FromBody] string stringifyData, [FromQuery] Guid token)
        {
            if (token != null)
            {
                var record = _context.Surveys.FirstOrDefault(e => e.UniqueId == token);

                if (record != null)
                {
                    record.UpdatedSurvey = stringifyData;
                }
                _context.SaveChanges();
                return Ok(new { success = true });
            }
            return BadRequest();
        }


        [HttpPost]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files, List<string> fileKey, List<string> fileToken)
        {
            try
            {
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments", fileToken[i], fileKey[i]);

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, file.FileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult BgImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image uploaded or the uploaded image is empty.");
            }

            // Check file size
            if (image.Length > 5 * 1024 * 1024) // 5 MB
            {
                return BadRequest("Uploaded image exceeds the maximum allowed size of 5 MB.");
            }

            // Check file type
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
            string fileExtension = Path.GetExtension(image.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Only JPG, JPEG, and PNG files are allowed.");
            }

            // Generate a unique filename
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);

            // Save the image to the server
            string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

            // Check if the uploads directory exists, if not, create it
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(stream);
            }

            // Construct the URL for accessing the uploaded image
            string imageUrl = "/uploads/" + uniqueFileName;

            // Return the URL of the uploaded image
            return Ok(new { imageUrl });
        }
    }
}
