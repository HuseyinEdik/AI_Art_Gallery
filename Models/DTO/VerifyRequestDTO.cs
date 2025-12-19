namespace AI_Art_Gallery.Models.DTO
{
    public class VerifyRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
    }
}
