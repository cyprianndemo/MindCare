namespace MindCare.MentalResources
{
    public class ResourceCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }
    }

    public class MentalHealthResource
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Tags { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string Author { get; set; }
    }

    public class TherapyReason
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }
    }
}
