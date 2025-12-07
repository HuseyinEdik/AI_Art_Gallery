using System.ComponentModel.DataAnnotations;

namespace AI_Art_Gallery.Models
{
    public class AppUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty; // Şifre

        public string Role { get; set; } = "User"; // Admin veya User

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // İlişkiler
        public ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}