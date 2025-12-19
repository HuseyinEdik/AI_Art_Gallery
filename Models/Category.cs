using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AI_Art_Gallery.Models
{
    public class Category
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
    }
}
