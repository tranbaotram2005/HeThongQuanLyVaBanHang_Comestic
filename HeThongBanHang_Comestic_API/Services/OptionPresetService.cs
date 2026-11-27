using System.Text.Json;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;

namespace HeThongBanHang_Comestic_API.Services
{
    public class OptionPresetService : IOptionPresetService
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

        public OptionPresetService(IWebHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "option-presets.json");
        }

        public async Task<List<OptionPresetDTO>> GetAllAsync()
        {
            await _lock.WaitAsync();
            try
            {
                var list = await ReadOrSeedUnlockedAsync();
                return list.OrderBy(x => x.Id).ToList();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<List<OptionPresetDTO>> GetActiveAsync()
        {
            var all = await GetAllAsync();
            return all.Where(x => x.IsActive).ToList();
        }

        public async Task<OptionPresetDTO?> GetByIdAsync(int id)
        {
            var all = await GetAllAsync();
            return all.FirstOrDefault(x => x.Id == id);
        }

        public async Task<(bool Success, string Message, OptionPresetDTO? Preset)> CreateAsync(OptionPresetUpsertDTO dto)
        {
            var cleanName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleanName))
                return (false, "Tên nhóm thuộc tính không được để trống.", null);

            var cleanValues = CleanValues(dto.Values);
            if (!cleanValues.Any())
                return (false, "Vui lòng nhập ít nhất 1 giá trị mẫu.", null);

            await _lock.WaitAsync();
            try
            {
                var list = await ReadOrSeedUnlockedAsync();
                if (list.Any(x => x.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
                    return (false, $"Option mẫu \"{cleanName}\" đã tồn tại.", null);

                int nextId = list.Any() ? list.Max(x => x.Id) + 1 : 1;
                var item = new OptionPresetDTO
                {
                    Id = nextId,
                    Name = cleanName,
                    Values = cleanValues,
                    Description = dto.Description?.Trim(),
                    IsActive = dto.IsActive
                };
                list.Add(item);
                await WriteUnlockedAsync(list);
                return (true, "Thêm Option mẫu thành công.", item);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, OptionPresetUpsertDTO dto)
        {
            var cleanName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleanName))
                return (false, "Tên nhóm thuộc tính không được để trống.");

            var cleanValues = CleanValues(dto.Values);
            if (!cleanValues.Any())
                return (false, "Vui lòng nhập ít nhất 1 giá trị mẫu.");

            await _lock.WaitAsync();
            try
            {
                var list = await ReadOrSeedUnlockedAsync();
                var item = list.FirstOrDefault(x => x.Id == id);
                if (item == null)
                    return (false, "Không tìm thấy Option mẫu.");

                if (list.Any(x => x.Id != id && x.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
                    return (false, $"Option mẫu \"{cleanName}\" đã tồn tại.");

                item.Name = cleanName;
                item.Values = cleanValues;
                item.Description = dto.Description?.Trim();
                item.IsActive = dto.IsActive;

                await WriteUnlockedAsync(list);
                return (true, "Cập nhật Option mẫu thành công.");
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            await _lock.WaitAsync();
            try
            {
                var list = await ReadOrSeedUnlockedAsync();
                var removed = list.RemoveAll(x => x.Id == id);
                if (removed == 0)
                    return (false, "Không tìm thấy Option mẫu.");

                await WriteUnlockedAsync(list);
                return (true, "Đã xóa Option mẫu.");
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<OptionPresetDTO>> ReadOrSeedUnlockedAsync()
        {
            if (!File.Exists(_filePath))
            {
                var defaults = GetDefaultPresets();
                await WriteUnlockedAsync(defaults);
                return defaults;
            }

            var json = await File.ReadAllTextAsync(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                var defaults = GetDefaultPresets();
                await WriteUnlockedAsync(defaults);
                return defaults;
            }

            return JsonSerializer.Deserialize<List<OptionPresetDTO>>(json) ?? new List<OptionPresetDTO>();
        }

        private async Task WriteUnlockedAsync(List<OptionPresetDTO> list)
        {
            var json = JsonSerializer.Serialize(list, JsonOpts);
            await File.WriteAllTextAsync(_filePath, json);
        }

        private static List<string> CleanValues(IEnumerable<string>? rawValues)
        {
            if (rawValues == null) return new List<string>();
            return rawValues
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<OptionPresetDTO> GetDefaultPresets() => new()
        {
            new OptionPresetDTO
            {
                Id = 1,
                Name = "Dung tích",
                Description = "Thể tích chai/lọ/tuýp nước hoa, mỹ phẩm",
                Values = new List<string> { "10ml", "30ml", "50ml", "75ml", "100ml", "200ml", "400ml" },
                IsActive = true
            },
            new OptionPresetDTO
            {
                Id = 2,
                Name = "Mùi hương",
                Description = "Nhóm hương cho nước hoa, xịt thơm, sữa tắm",
                Values = new List<string> { "Hoa Hồng", "Gỗ Tuyết Tùng", "Cam Bergamot", "Vani", "Xạ Hương", "Trà Xanh" },
                IsActive = true
            },
            new OptionPresetDTO
            {
                Id = 3,
                Name = "Màu sắc",
                Description = "Bảng màu son môi, phấn má, kẻ mắt",
                Values = new List<string> { "A01 - Đỏ Dâu", "A02 - Đỏ Cam", "A06 - Đỏ Nâu", "A12 - Nâu Gạch", "Hồng Đào" },
                IsActive = true
            },
            new OptionPresetDTO
            {
                Id = 4,
                Name = "Tông da",
                Description = "Tông nền kem nền, cushion, che khuyết điểm",
                Values = new List<string> { "21 - Sáng (Light)", "23 - Tự nhiên (Natural)", "Ấm (Warm)" },
                IsActive = true
            },
            new OptionPresetDTO
            {
                Id = 5,
                Name = "Phiên bản",
                Description = "Quy cách đóng gói hoặc phiên bản sản phẩm",
                Values = new List<string> { "Tiêu chuẩn", "Mini Size", "Full Size", "Gift Set" },
                IsActive = true
            }
        };
    }
}
