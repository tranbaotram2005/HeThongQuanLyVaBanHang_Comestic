using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IOptionPresetService
    {
        Task<List<OptionPresetDTO>> GetAllAsync();
        Task<List<OptionPresetDTO>> GetActiveAsync();
        Task<OptionPresetDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message, OptionPresetDTO? Preset)> CreateAsync(OptionPresetUpsertDTO dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, OptionPresetUpsertDTO dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }
}
