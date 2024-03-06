namespace SHL_Platform.Models
{
    public class Survey
    {
        public int Id { get; set; }
        public string FormDataJson { get; set; }
        public string? SendToEmail { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedSurvey { get; set; }
        public Guid UniqueId { get; set; }
        public string? Bcc { get; set; }
        public string? Cc { get; set; }
        public string? SurveyTags { get; set; }
    }

}
