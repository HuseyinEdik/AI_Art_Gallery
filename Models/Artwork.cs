using System;
using System.Collections.Generic;

namespace AI_Art_Gallery.Models
{
    public class Artwork
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string PromptText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int AppUserId { get; set; }
        public AppUser? AppUser { get; set; }  // ? → null olabilir

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
