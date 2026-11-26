using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IAccountService
    {
        
         Task<LoginResponseDTO?> LoginAsync(LoginDTO model);
        Task<bool> RegisterAsync(RegisterDTO model);
    }
}
