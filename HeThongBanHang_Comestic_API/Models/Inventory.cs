using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class Inventory
{
    public int Id { get; set; }

    public int VariantId { get; set; }

    public int? Quantity { get; set; }

    public int? WarehouseId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Notes { get; set; }

    public int? Status { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual ProductVariant Variant { get; set; } = null!;
}
