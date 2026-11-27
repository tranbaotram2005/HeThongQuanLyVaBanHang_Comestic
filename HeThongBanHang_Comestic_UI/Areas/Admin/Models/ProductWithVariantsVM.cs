using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HeThongBanHang_Comestic_UI.Areas.Admin.Models
{
    public class ProductVariantFormItem
    {
        public int Id { get; set; }
        public string? Sku { get; set; }
        public string? Name { get; set; }
        public string? Option1Value { get; set; }
        public string? Option2Value { get; set; }
        public string? Description { get; set; }
        public string? Barcode { get; set; }
        public decimal? Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public int? StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsDefault { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public int Status { get; set; } = 1;
        public int OriginalStatus { get; set; } = 1;
    }

    public class ProductCreateWithVariantsVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        public string ProductName { get; set; } = string.Empty;

        public string? Brand { get; set; }
        public string? Description { get; set; }
        public string? QuantityPerUnit { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp.")]
        public int? SupplierId { get; set; }

        public decimal? UnitPrice { get; set; }
        public short? UnitsInStock { get; set; }
        public string? Picture { get; set; }
        public string? Notes { get; set; }
        public IFormFile? ImageFile { get; set; }

        public string? Option1Name { get; set; }
        public string? Option2Name { get; set; }

        public List<ProductVariantFormItem> Variants { get; set; } = new();
    }

    public class ProductEditWithVariantsVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        public string ProductName { get; set; } = string.Empty;

        public string? Brand { get; set; }
        public string? Description { get; set; }
        public string? QuantityPerUnit { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp.")]
        public int? SupplierId { get; set; }

        public decimal? UnitPrice { get; set; }
        public short? UnitsInStock { get; set; }
        public string? Picture { get; set; }
        public string? Notes { get; set; }
        public IFormFile? ImageFile { get; set; }

        public string? Option1Name { get; set; }
        public string? Option2Name { get; set; }

        public List<ProductVariantFormItem> Variants { get; set; } = new();
    }
}
