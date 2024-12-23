namespace MindCare.Models
{
    public class Medication
    {
        public int MedicationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Dosage { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public virtual ICollection<Prescription> Prescriptions { get; set; }
    }
}