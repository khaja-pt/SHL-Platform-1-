using System.Diagnostics.Metrics;

namespace SHL_Platform.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string EmployeeName { get; set; }
        public required string Email { get; set; }
        public required string CompanyName { get; set; }
        public required string Gender { get; set; }
        public required string Password { get; set; }
        public bool IsAdmin { get; set; }
        public int CountryId { get; set; }
        public required bool RequiresPasswordChange { get; set; }
        public Country Country { get; set; }
    }
}
