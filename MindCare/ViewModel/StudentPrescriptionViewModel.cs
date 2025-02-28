namespace MindCare.ViewModel
{
    public class StudentPrescriptionViewModel
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public List<PrescriptionDetailViewModel> Prescriptions { get; set; }
    }
}
