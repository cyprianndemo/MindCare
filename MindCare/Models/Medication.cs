using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class Medication
    {
        [Key]
        public int MedicationId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Dosage { get; set; }

        [Required]
        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }

        public string ImagePath { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        // Many-to-Many Relationship
        public ICollection<PrescriptionMedication> PrescriptionMedications { get; set; }
    }
}