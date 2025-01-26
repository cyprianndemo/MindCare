namespace MindCare.Models
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; }
        public string Model { get; set; } = "gpt-3.5-turbo";
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 150;
    }
}
