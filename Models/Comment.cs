using System.ComponentModel.DataAnnotations;

namespace AI_Art_Gallery.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // İlişkiler
        public int ArtworkId { get; set; }
        public Artwork? Artwork { get; set; }

        public int AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
    }
}