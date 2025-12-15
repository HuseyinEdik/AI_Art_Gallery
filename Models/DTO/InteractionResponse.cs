namespace AI_Art_Gallery.Models.DTO
{
    public class InteractionResponse
    {
        public string Message { get; set; } = string.Empty;
        public bool IsLiked { get; set; }
        public int LikeCount { get; set; }
    }
}
