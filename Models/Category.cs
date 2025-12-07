using System.ComponentModel.DataAnnotations;

namespace AI_Art_Gallery.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        public string Name { get; set; } = string.Empty;

        // İlişki: Bir kategoride birden fazla resim olabilir
        public ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
    }
}