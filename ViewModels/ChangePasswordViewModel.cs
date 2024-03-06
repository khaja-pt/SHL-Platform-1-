using System.ComponentModel.DataAnnotations;

namespace SHL_Platform.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "The {0} must be at least {1} characters long.")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}
