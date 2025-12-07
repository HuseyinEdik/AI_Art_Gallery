using System.ComponentModel.DataAnnotations;

namespace AI_Art_Gallery.Models
{
    public class AppLog
    {
        [Key]
        public int Id { get; set; }

        public string Action { get; set; } = string.Empty; // Örn: "Giriş Yapıldı", "Resim Silindi"
        public string Details { get; set; } = string.Empty; // Detaylı bilgi
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}