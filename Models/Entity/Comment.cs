namespace AI_Art_Gallery.Models.Entity
{
    public class Comment
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public User User { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
