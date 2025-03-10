using MindCare.ViewModel;

namespace MindCare.PsychiatristReport
{
    public class PsychiatristReportViewModel
    {
        public string PsychiatristName { get; set; }
        public int TotalSessions { get; set; }
        public int PendingSessions { get; set; }
        public int ApprovedSessions { get; set; }
        public List<PrescriptionViewModel> Prescriptions { get; set; }
        public DateTime GeneratedDate { get; set; }
    }

   
}
