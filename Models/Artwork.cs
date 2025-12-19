using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AI_Art_Gallery.Models
{
    public class Artwork
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        // ImageUrl çok uzun olabilir, ancak gerektiğinde kullanılmalı
        [StringLength(100000)] // Base64 veya uzun URL'ler için
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;
        
        [StringLength(2000)]
        [JsonPropertyName("promptText")]
        public string PromptText { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("appUserId")]
        public int AppUserId { get; set; }
        
        // Spring Boot farklı formatlardan birini döndürebilir
        [JsonPropertyName("appUser")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AppUser? AppUser { get; set; }
        
        // Alternatif: user field name
        [JsonPropertyName("user")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AppUser? User 
        { 
            get => AppUser; 
            set => AppUser = value; 
        }

        // Navigation properties - Circular reference önlemek için JsonIgnore
        [JsonPropertyName("comments")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        
        [JsonPropertyName("likes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        
        [JsonPropertyName("categories")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        
        // API'den gelen beğeni durumu
        [JsonPropertyName("isLikedByCurrentUser")]
        public bool IsLikedByCurrentUser { get; set; }
    }
}
