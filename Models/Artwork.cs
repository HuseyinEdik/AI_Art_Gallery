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
        
        // Spring Boot "user" olarak döndürüyor (appUser değil)
        [JsonPropertyName("user")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AppUser? AppUser { get; set; }
        
        // Spring Boot "category" olarak döndürüyor (tek object, array değil)
        [JsonPropertyName("category")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Category? Category { get; set; }
        
        // Categories collection'ı Category'den türetiyoruz
        [JsonIgnore]
        public ICollection<Category> Categories 
        { 
            get 
            {
                if (Category == null) return new List<Category>();
                return new List<Category> { Category };
            }
            set 
            {
                if (value != null && value.Any())
                {
                    Category = value.First();
                }
            }
        }
        
        // Spring Boot array yerine sayı döndürüyor
        [JsonPropertyName("likeCount")]
        public int LikeCount { get; set; }
        
        [JsonPropertyName("commentCount")]
        public int CommentCount { get; set; }
        
        // Likes ve Comments artık kullanılmayacak (sayı olarak geldiği için)
        [JsonIgnore]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        
        [JsonIgnore]
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        
        // API'den gelen beğeni durumu
        [JsonPropertyName("isLikedByCurrentUser")]
        public bool IsLikedByCurrentUser { get; set; }
    }
}
