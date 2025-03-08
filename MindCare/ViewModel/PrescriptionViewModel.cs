using Microsoft.AspNetCore.Mvc.Rendering;
using MindCare.Models;
using System.ComponentModel.DataAnnotations;

namespace MindCare.ViewModel
{
    public class PrescriptionViewModel
    {
        public int PrescriptionId { get; set; }
        
        [Required]
        public int MedicationId { get; set; }
        
        public string MedicationName { get; set; }
        
        [Required(ErrorMessage = "Patient is required")]
        public string StudentId { get; set; }
        
        public string PsychiatristId { get; set; }
        
        [Required(ErrorMessage = "Dosage is required")]
        public string Dosage { get; set; }
        
        [Required(ErrorMessage = "Frequency is required")]
        public string Frequency { get; set; }
        
        [Required(ErrorMessage = "Duration is required")]
        public string Duration { get; set; }
        
        [Required(ErrorMessage = "Instructions are required")]
        public string Instructions { get; set; }
        
        [Required(ErrorMessage = "Prescription date is required")]
        [DataType(DataType.Date)]
        public DateTime PrescriptionDate { get; set; }
        
        public string Status { get; set; }
        
        // Navigation properties
        public virtual Medication Medication { get; set; }
        public virtual ApplicationUser Student { get; set; }
        public virtual ApplicationUser Psychiatrist { get; set; }
        
        // For dropdown lists
        public List<SelectListItem> Patients { get; set; }

        public PrescriptionViewModel()
        {
            PrescriptionDate = DateTime.UtcNow;
            Status = "Active";
            Patients = new List<SelectListItem>();
        }
    }
}