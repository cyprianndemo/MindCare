using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class Medication
    {
        [Key]
        public int MedicationId { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Description { get; set; } // Description of the medication

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; } // Quantity in stock

        // Navigation Property
        public ICollection<Prescription> Prescriptions { get; set; } // Link to prescriptions

        public Medication()
        {
            Prescriptions = new List<Prescription>();
        }
    }
}
