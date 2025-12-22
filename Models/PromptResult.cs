namespace AI_Art_Gallery.Models
{
    public class PromptResult
    {
        public string Tahmin { get; set; } = string.Empty;
        public string Guven_Orani { get; set; } = string.Empty;
        public Dictionary<string, double> Detaylar { get; set; } = new Dictionary<string, double>();
    }
}

