using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SHL_Platform.ViewModels
{
    public class RegistrationViewModel
    {
        [Required]
        public string EmployeeName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public bool IsAdmin { get; set; }
        [Required]
        public int CountryId { get; set; }
        public SelectList Countries { get; set; }
    }
}
