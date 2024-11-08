using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class ApplicationUser : IdentityUser
    {
        
             [Required]
            [StringLength(50)]
            public string? FirstName { get; set; }

            [Required]
            [StringLength(50)]
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
            public decimal Rating { get; set; }

        [Display(Name = "Verification Code")]
        public string? VerificationCode { get; set; }
      
    
    }


    // Constructor

   /* public ApplicationUser()
    {
        Appointments = new List<Appointment>();
        Payments = new List<Payment>();
        Resources = new List<MentalHealthResource>();
    }*/

}

