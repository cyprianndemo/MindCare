namespace MindCare.ViewModel
{
    public class PrescriptionDetailViewModel
    {
        public int PrescriptionId { get; set; }
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Duration { get; set; }
        public string Instructions { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public string Status { get; set; }
        public string PsychiatristName { get; set; }
    }
}
