using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class ViewProductVariantDetail
{
    public int ProductVariantId { get; set; }

    public int? ProductId { get; set; }

    public string? Sku { get; set; }

    public decimal? Price { get; set; }

    public int? StockQuantity { get; set; }

    public string? VariantOptions { get; set; }
}
