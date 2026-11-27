namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class LoginResponseDTO
    {
        public string AccessToken { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();
    }
}
