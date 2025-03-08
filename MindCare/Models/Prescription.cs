using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }

        public int MedicationId { get; set; }
        public Medication Medication { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty; 

        public DateTime DatePrescribed { get; set; } = DateTime.Now;

        [Required]
        public string Instructions { get; set; } = string.Empty; 

        public string StudentId { get; set; } = string.Empty; 

        public string PsychiatristId { get; set; } = string.Empty; 

        [Required]
        public DateTime PrescribedDate { get; set; } = DateTime.UtcNow; 

        public string Dosage { get; set; } = string.Empty; // Default value to avoid null

        public string Frequency { get; set; } = string.Empty; // Default value to avoid null

        public string Duration { get; set; } = string.Empty; // Default value to avoid null

        public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow; // Default value

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default value

        public string Status { get; set; } = "Active"; // Default value

        public virtual ApplicationUser Student { get; set; }
        public virtual ApplicationUser Psychiatrist { get; set; }

        // Nullable with default empty collection
        public virtual ICollection<PrescriptionMedication> PrescriptionMedications { get; set; } = new List<PrescriptionMedication>();
    }

}
