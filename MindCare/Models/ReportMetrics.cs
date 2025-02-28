namespace MindCare.Models
{
    public class ReportMetrics
    {
        public int Id { get; set; }
        public DateTime Month { get; set; }
        public string UserId { get; set; }
        public string UserRole { get; set; }

        // Platform Performance Metrics
        public int SessionsCompleted { get; set; }
        public double AverageSessionDuration { get; set; }
        public int ResourcesAccessed { get; set; }

        // Mental Health Metrics
        public int MoodScoreAverage { get; set; }
        public int AnxietyScoreAverage { get; set; }
        public int WellbeingScoreAverage { get; set; }
        public string TherapistNotes { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}
