using HeThongBanHang_Comestic_UI.DTOs;
namespace HeThongBanHang_Comestic_UI.Models
{
    public class HomeViewModels
    {
        public IEnumerable<CategoryResponseDTO> Categories { get; set; } = Enumerable.Empty<CategoryResponseDTO>();
        public IEnumerable<ProductResponseDTO> Products { get; set; } = Enumerable.Empty<ProductResponseDTO>();
        public IEnumerable<SupplierResponseDTO> Suppliers { get; set; } = Enumerable.Empty<SupplierResponseDTO>();
    }
}
