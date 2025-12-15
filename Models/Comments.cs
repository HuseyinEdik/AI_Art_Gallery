using System;

namespace AI_Art_Gallery.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ArtworkId { get; set; }
        public Artwork? Artwork { get; set; }

        public int AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
    }
}
