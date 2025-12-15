namespace AI_Art_Gallery.Models.Entity
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
    }
}
