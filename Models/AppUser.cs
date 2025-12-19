using System.Text.Json.Serialization;

namespace AI_Art_Gallery.Models
{
    public class AppUser
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        
        [JsonPropertyName("surname")]
        public string Surname { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("verificationCode")]
        public string? VerificationCode { get; set; }
        
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        // UI için basit Role alanı
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("roles")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }

    public class Role
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
