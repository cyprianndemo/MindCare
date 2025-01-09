using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MindCare.ViewModel
{
    public class MedicationViewModel
    {
        public int MedicationId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Dosage is required")]
        public string Dosage { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be between $0.01 and $10,000.00")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Medication Image")]
        [DataType(DataType.Upload)]
        public IFormFile MedicationImage { get; set; }

        //public string ExistingImagePath { get; set; }
    }
}