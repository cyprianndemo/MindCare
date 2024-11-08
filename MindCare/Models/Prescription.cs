using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }

        [Required]
        public string? Description { get; set; } // Description of the prescribed treatment

        public DateTime DatePrescribed { get; set; } = DateTime.Now;

        // Foreign Keys
        [ForeignKey("Student")]
        public string? StudentId { get; set; }
        public Student Student { get; set; } // Student receiving the prescription

        [ForeignKey("Psychiatrist")]
        public string? PsychiatristId { get; set; }
        public Psychiatrist Psychiatrist { get; set; } // Psychiatrist issuing the prescription

        // Navigation Property
        public ICollection<Medication> Medications { get; set; }

        public Prescription()
        {
            Medications = new List<Medication>();
        }
    }
}
