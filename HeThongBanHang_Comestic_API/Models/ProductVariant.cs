using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class ProductVariant
{
    public int Id { get; set; }

    public int? ProductId { get; set; }

    public string? Sku { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Barcode { get; set; }

    public decimal? Price { get; set; }

    public decimal? CompareAtPrice { get; set; }

    public int? StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsDefault { get; set; }

    public decimal? Weight { get; set; }

    public decimal? Length { get; set; }

    public decimal? Width { get; set; }

    public decimal? Height { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Notes { get; set; }

    public int? Status { get; set; }

    public byte[] RowVersions { get; set; } = null!;

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product? Product { get; set; }

    public virtual ICollection<ProductVariantOptionValue> ProductVariantOptionValues { get; set; } = new List<ProductVariantOptionValue>();

    public virtual User? UpdatedByNavigation { get; set; }
}
