namespace AI_Art_Gallery.Models.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
