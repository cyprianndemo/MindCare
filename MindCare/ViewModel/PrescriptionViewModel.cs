using Microsoft.AspNetCore.Mvc.Rendering;
using MindCare.Models;
using System.ComponentModel.DataAnnotations;

namespace MindCare.ViewModel
{
    public class PrescriptionViewModel
    {
        public int MedicationId { get; set; }
        public string MedicationName { get; set; }
        public int PatientId { get; set; }
        public string StudentId { get; set; }   
        [Required]
        public string Dosage { get; set; }
        [Required]
        public string Frequency { get; set; }
        [Required]
        public string Duration { get; set; }
        [Required]
        public string Instructions { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public string Status { get; set; }
        public int PrescriptionId { get; set; }
        public Medication Medication { get; set; }
        public virtual ApplicationUser Student { get; set; }

        public List<SelectListItem> Patients { get; set; }
        public virtual ApplicationUser Psychiatrist { get; set; }
        public string PsychiatristId { get; set; }

        public PrescriptionViewModel()
        {
            PrescriptionDate = DateTime.Now;
            Patients = new List<SelectListItem>();
        }
    }
}
