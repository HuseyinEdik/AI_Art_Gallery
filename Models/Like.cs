using System.ComponentModel.DataAnnotations;

namespace AI_Art_Gallery.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;

        // İlişkiler
        public int ArtworkId { get; set; }
        public Artwork? Artwork { get; set; }

        public int AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
    }
}