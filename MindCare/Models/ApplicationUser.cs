using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class ApplicationUser : IdentityUser
    {

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string? FirstName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string? LastName { get; set; }


        [Required]
            public string? Role { get; set; }

            public string? University { get; set; }
            public string? Course { get; set; }
            public string? YearOfStudy { get; set; }

            public string? NationalId { get; set; }
            public string? LicenceNumber { get; set; }
            public string? Hospital { get; set; }
            public string? Specialization { get; set; }
            public virtual ICollection<Notification> Notifications { get; set; }

        public decimal Rating { get; set; }

        [Display(Name = "Verification Code")]
        public string? VerificationCode { get; set; }
        public string? CurrentMood { get; set; }
        public string? CurrentMoodIcon { get; set; }
        public DateTime? LastLoginDate { get; set; }

    }


    // Constructor

   /* public ApplicationUser()
    {
        Appointments = new List<Appointment>();
        Payments = new List<Payment>();
        Resources = new List<MentalHealthResource>();
    }*/

}

