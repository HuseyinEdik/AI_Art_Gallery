using AI_Art_Gallery.Models.ViewModels;

namespace AI_Art_Gallery.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public List<DetailedArtViewModel> DetailedArts { get; set; } = new();
        public List<CategoryStatViewModel> CategoryStats { get; set; } = new();
        public List<ActiveUserViewModel> ActiveUsers { get; set; } = new();
        public List<RecentUploadViewModel> RecentUploads { get; set; } = new();
        public List<LogSummaryViewModel> LogSummary { get; set; } = new();
    }
}
