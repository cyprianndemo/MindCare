namespace MindCare.ViewModel
{
    public class SystemUsageReportViewModel
    {
        public int TotalSessions { get; set; }  // Total number of sessions
        public int CurrentMonthSessions { get; set; }  // Sessions in the current month
        public List<MonthlySession> MonthlySessionData { get; set; }  // List of monthly session data
    }

    public class MonthlySession
    {
        public string Month { get; set; }  // Month in "YYYY-MM" format
        public int SessionCount { get; set; }  // Number of sessions for the month
    }

}
