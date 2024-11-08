namespace MindCare.Models
{
    public class Therapist : ApplicationUser
    {
        public string? Specialization { get; set; }
        public string? Availability { get; set; } // For example: "Weekdays 9 AM - 5 PM"
    }

}
