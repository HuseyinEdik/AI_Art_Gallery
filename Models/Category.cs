using System.Collections.Generic;

namespace AI_Art_Gallery.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
    }
}
