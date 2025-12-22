namespace AI_Art_Gallery.Models.ViewModels
{
    public class DetailedArtViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
    }
}
