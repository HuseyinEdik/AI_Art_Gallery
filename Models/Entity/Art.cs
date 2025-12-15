namespace AI_ART_GALLERY.Models.Entity
{
    public class Art
    {
        public long Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        // Oluşturan kullanıcı
        public string CreatedBy { get; set; } = string.Empty;

        // Tarih
        public DateTime CreatedAt { get; set; }
    }
}
