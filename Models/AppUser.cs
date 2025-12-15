namespace AI_Art_Gallery.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string? VerificationCode { get; set; }
        public bool Enabled { get; set; }

        // UI için basit Role alanı
        public string? Role { get; set; }

        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
