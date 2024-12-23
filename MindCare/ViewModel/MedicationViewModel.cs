using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MindCare.ViewModel
{
    public class MedicationViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Dosage { get; set; }

        [Display(Name = "Medication Image")]
        public IFormFile MedicationImage { get; set; }
    }
}