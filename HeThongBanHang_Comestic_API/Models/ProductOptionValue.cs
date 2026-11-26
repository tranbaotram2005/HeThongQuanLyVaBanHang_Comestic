using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class ProductOptionValue
{
    public int Id { get; set; }

    public int? ProductOptionId { get; set; }

    public string Name { get; set; } = null!;

    public int? SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Notes { get; set; }

    public int? Status { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ProductOption? ProductOption { get; set; }

    public virtual ICollection<ProductVariantOptionValue> ProductVariantOptionValues { get; set; } = new List<ProductVariantOptionValue>();

    public virtual User? UpdatedByNavigation { get; set; }
}
