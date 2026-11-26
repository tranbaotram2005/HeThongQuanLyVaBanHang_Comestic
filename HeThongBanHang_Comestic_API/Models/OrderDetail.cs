using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class OrderDetail
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int? ProductVariantId { get; set; }

    public decimal UnitPrice { get; set; }

    public short Quantity { get; set; }

    public string? ProductName { get; set; }

    public string? Sku { get; set; }

    public string? VariantInfo { get; set; }

    public decimal? LineTotal { get; set; }

    public float? Discount { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Notes { get; set; }

    public int? Status { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ProductVariant? ProductVariant { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
