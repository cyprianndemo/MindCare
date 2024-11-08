namespace MindCare.Models
{
    public class Psychiatrist : ApplicationUser
    {
        public string? LicenseNumber { get; set; }
        public string? ConsultationType { get; set; } // e.g., General, Trauma, etc.
    }
}
