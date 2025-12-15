namespace AI_Art_Gallery.Models
{
    public class Like
    {
        public int Id { get; set; }

        public int ArtworkId { get; set; }
        public Artwork? Artwork { get; set; }

        public int AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
    }
}
