namespace HeThongBanHang_Comestic_API.DTOs
{
    public class VariantOptionValuePairDTO
    {
        public string OptionName { get; set; } = string.Empty;
        public string OptionValue { get; set; } = string.Empty;
    }

    public class VariantOptionsSyncItemDTO
    {
        public string Sku { get; set; } = string.Empty;
        public List<VariantOptionValuePairDTO> Options { get; set; } = new();
    }

    public class ProductOptionsSyncDTO
    {
        public List<string> OptionNames { get; set; } = new();
        public List<VariantOptionsSyncItemDTO> Variants { get; set; } = new();
    }
}
