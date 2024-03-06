using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SHL_Platform.Data;
using SHL_Platform.Models;
using SHL_Platform.Services;
using SHL_Platform.ViewModels;
using System.Security.Claims;

namespace SHL_Platform.Controllers
{
    public class AccountController(ApplicationContext context) : Controller
    {
        private readonly ApplicationContext _context = context;

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the user from the database by username
                var admin = _context.Admins.FirstOrDefault(u => u.Username == model.Username);
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Username && u.IsAdmin == false);
                if (admin != null)
                {
                    // Hash the password provided by the user
                    var hashedPassword = SecurityHelper.HashPassword(model.Password);

                    // Compare the hashed password with the stored hashed password
                    if (hashedPassword == admin.Password)
                    {
                        // Set user's identity upon successful login
                        var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, admin.Username)
                    // You can add additional claims as needed, such as roles, etc.
                };

                        var userIdentity = new ClaimsIdentity(claims, "login");

                        ClaimsPrincipal principal = new(userIdentity);
                        HttpContext.SignInAsync(principal).Wait(); // Sign in the user

                        // Check if the user requires password change
                        if (admin.RequiresPasswordChange)
                        {
                            // Redirect to password change page
                            return RedirectToAction("ChangePassword", "Account");
                        }

                        // Authentication successful, redirect to home page or any other page
                        return RedirectToAction("Index", "Home");
                    }
                }
                else if (user != null)
                {
                    // Hash the password provided by the user
                    var hashedPassword = SecurityHelper.HashPassword(model.Password);

                    // Compare the hashed password with the stored hashed password
                    if (hashedPassword == user.Password)
                    {
                        // Set user's identity upon successful login
                        var claims = new List<Claim> {
                                new(ClaimTypes.Name, user.Email)
                                // You can add additional claims as needed, such as roles, etc.
                        };

                        var userIdentity = new ClaimsIdentity(claims, "login");

                        ClaimsPrincipal principal = new(userIdentity);
                        HttpContext.SignInAsync(principal).Wait(); // Sign in the user

                        // Check if the user requires password change
                        if (user.RequiresPasswordChange)
                        {
                            // Redirect to password change page
                            return RedirectToAction("ChangePassword", "Account");
                        }
                        return RedirectToAction("UserPage", "Home");

                    }
                }
                // Authentication failed, return to login page with error message
                ModelState.AddModelError("", "Invalid username or password");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult RegisterUser()
        {
            var viewModel = new RegistrationViewModel
            {
                Countries = new SelectList(_context.Countries.ToList(), "Id", "Name")
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegistrationViewModel viewModel, [FromServices] EmailService emailservices)
        {
            // Populate the Countries dropdown
            viewModel.Countries = new SelectList(_context.Countries.ToList(), "Id", "Name", viewModel.CountryId);

            // Manually remove the ModelState error for Countries
            ModelState.Remove("Countries");
            // Check if the email already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == viewModel.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already in use.");
            }

            if (ModelState.IsValid)
            {
                var randomPassword = SecurityHelper.GenerateRandomPassword();
                // Hash the random password
                var hashedPassword = SecurityHelper.HashPassword(randomPassword);

                var user = new User
                {
                    EmployeeName = viewModel.EmployeeName,
                    Email = viewModel.Email,
                    CompanyName = viewModel.CompanyName,
                    Gender = viewModel.Gender,
                    IsAdmin = viewModel.IsAdmin, // Bind directly to IsAdmin
                    CountryId = viewModel.CountryId,
                    Password = hashedPassword,
                    RequiresPasswordChange = true
                };
                _context.Users.Add(user);
                if (viewModel.IsAdmin)
                {
                    var admin = new Admin
                    {
                        Username = viewModel.Email,
                        Password = hashedPassword,
                        RequiresPasswordChange = true
                    };
                    _context.Admins.Add(admin);
                }
                _context.SaveChanges();

                var emailBody = $"Welcome to SHL Platform ! Your temporary password is: {randomPassword}. Please log in using this password and change it after first login.";
                await emailservices.SendEmailAsync(viewModel.Email, "Welcome", emailBody);
                ViewData["SuccessMessage"] = "Registration successful";
            }

            return View(viewModel);
        }

        public IActionResult ChangePassword()
        {
            var viewModel = new ChangePasswordViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the logged-in user from your data source
                var admin = _context.Admins.FirstOrDefault(u => u.Username == User.Identity.Name);
                var user = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name && u.IsAdmin == false);
                if (admin != null)
                {
                    var hashedPassword = SecurityHelper.HashPassword(viewModel.NewPassword);
                    // Update the user's password and set RequiresPasswordChange to false
                    admin.Password = hashedPassword;
                    admin.RequiresPasswordChange = false;
                    _context.SaveChanges();
                    var successMessage = $"Password Changed Successfully ! Please <a href='{Url.Action("Login", "Account")}'>login</a> with new password";
                    ViewData["SuccessMessage"] = successMessage;
                    // Redirect to a page indicating successful password change
                    return View(viewModel);

                }
                else if (user != null)
                {
                    var hashedPassword = SecurityHelper.HashPassword(viewModel.NewPassword);
                    // Update the user's password and set RequiresPasswordChange to false
                    user.Password = hashedPassword;
                    user.RequiresPasswordChange = false;
                    _context.SaveChanges();
                    var successMessage = $"Password Changed Successfully ! Please <a href='{Url.Action("Login", "Account")}'>login</a> with new password";
                    ViewData["SuccessMessage"] = successMessage;
                    // Redirect to a page indicating successful password change
                    return View(viewModel);
                }

                // If the user is not found, you may want to handle it differently, such as displaying an error message
                ModelState.AddModelError("", "User not found.");
            }

            // If ModelState is invalid, redisplay the form with validation errors
            return View(viewModel);
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email, [FromServices] EmailService emailservices)
        {
            // Find the user by email
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                var randomPassword = SecurityHelper.GenerateRandomPassword();

                var emailBody = $"Your new temporary password is: {randomPassword}. Please log in using this password and change it after the login.";

                // Send reset password email
                await emailservices.SendEmailAsync(user.Email, "Your New Password", emailBody);


                user.Password = SecurityHelper.HashPassword(randomPassword);
                if (user.IsAdmin)
                {
                    var admin = _context.Admins.FirstOrDefault(u => u.Username == user.EmployeeName);
                    if (admin != null)
                    {
                        admin.Password = SecurityHelper.HashPassword(randomPassword);
                    }
                }
                _context.SaveChanges();
                // Redirect to a page indicating that the reset password email has been sent
                return RedirectToAction("Login");
            }
            else
            {
                // If user not found, display error message or redirect to login page
                ModelState.AddModelError("", "User with this email does not exist.");
                return View();
            }
        }


        // Registration from Admin menu
        public IActionResult AddUser()
        {
            var viewModel = new RegistrationViewModel
            {
                Countries = new SelectList(_context.Countries.ToList(), "Id", "Name")
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(RegistrationViewModel viewModel, [FromServices] EmailService emailservices)
        {
            // Populate the Countries dropdown
            viewModel.Countries = new SelectList(_context.Countries.ToList(), "Id", "Name", viewModel.CountryId);

            // Manually remove the ModelState error for Countries
            ModelState.Remove("Countries");
            // Check if the email already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == viewModel.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already in use.");
            }

            if (ModelState.IsValid)
            {
                var randomPassword = SecurityHelper.GenerateRandomPassword();
                // Hash the random password
                var hashedPassword = SecurityHelper.HashPassword(randomPassword);

                var user = new User
                {
                    EmployeeName = viewModel.EmployeeName,
                    Email = viewModel.Email,
                    CompanyName = viewModel.CompanyName,
                    Gender = viewModel.Gender,
                    IsAdmin = false, // Bind directly to IsAdmin
                    CountryId = viewModel.CountryId,
                    Password = hashedPassword,
                    RequiresPasswordChange = true
                };
                _context.Users.Add(user);
                _context.SaveChanges();

                var emailBody = $"Welcome to SHL Platform ! Your temporary password is: {randomPassword}. Please log in using this password and change it after first login.";
                await emailservices.SendEmailAsync(viewModel.Email, "Welcome", emailBody);
                ViewData["SuccessMessage"] = "User Added successfully";
            }

            return View(viewModel);
        }


    }
}
