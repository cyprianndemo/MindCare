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
        public string? Description { get; set; }

        public DateTime DatePrescribed { get; set; } = DateTime.Now;

        [Required]
        public string Instructions { get; set; }
        public string StudentId { get; set; }
        public string PsychiatristId { get; set; }

        [Required]
        public DateTime PrescribedDate { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Duration { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public virtual ApplicationUser Student { get; set; }
        public virtual ApplicationUser Psychiatrist { get; set; }
        public ICollection<PrescriptionMedication> PrescriptionMedications { get; set; }
        public Prescription()
        {
            PrescriptionMedications = new List<PrescriptionMedication>();
        }
    }

}
