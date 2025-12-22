using System;
using System.Text.Json.Serialization;

namespace AI_Art_Gallery.Models
{
    public class Comment
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("artworkId")]
        public int ArtworkId { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Artwork? Artwork { get; set; }

        [JsonPropertyName("appUserId")]
        public int AppUserId { get; set; }
        
        // API "user" olarak döndürüyor (appUser değil - Artwork ile tutarlılık için)
        [JsonPropertyName("user")]
        public AppUser? AppUser { get; set; }
    }
}
