using System.ComponentModel.DataAnnotations;

namespace AI_Art_Gallery.Models
{
    public class Artwork
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        public string Title { get; set; } = string.Empty; // Varsayılan değer atadık

        public string ImageUrl { get; set; } = string.Empty; // Varsayılan değer

        [Required(ErrorMessage = "Prompt metni zorunludur.")]
        public string PromptText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // İlişkiler
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public int? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}