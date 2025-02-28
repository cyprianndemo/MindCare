namespace MindCare.Models
{
    // Add these models to your Models folder
    public class MentalHealthExercise
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Category { get; set; }
        public List<string> RelatedResourceCategories { get; set; }
        public int DurationMinutes { get; set; }
        public string DifficultyLevel { get; set; }
        public bool IsInteractive { get; set; }
    }

    public class ProfessionalResource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Specialty { get; set; }
        public string Description { get; set; }
        public string ContactUrl { get; set; }
        public List<string>? Categories { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public bool IsRemoteAvailable { get; set; }
    }

    public class MentalHealthAssessment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int QuestionCount { get; set; }
        public int TimeMinutes { get; set; }
        public string Category { get; set; }
        public int Order { get; set; }
        public string AssessmentUrl { get; set; }
    }
}
