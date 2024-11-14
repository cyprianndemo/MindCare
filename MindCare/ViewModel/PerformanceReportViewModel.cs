namespace MindCare.ViewModel
{
    public class PerformanceReportViewModel
    {
        public int StudentCount { get; set; }
        public int TherapistCount { get; set; }
        public int PsychiatristCount { get; set; }

        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int UpcomingSessions { get; set; }
    }

}
