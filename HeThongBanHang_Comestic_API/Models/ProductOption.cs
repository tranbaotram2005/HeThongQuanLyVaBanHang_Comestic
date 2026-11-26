using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class ProductOption
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public int? SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Notes { get; set; }

    public int? Status { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductOptionValue> ProductOptionValues { get; set; } = new List<ProductOptionValue>();

    public virtual User? UpdatedByNavigation { get; set; }
}
