using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class ViewProductVariantFullDetail
{
    public int ProductVariantId { get; set; }

    public int? ProductId { get; set; }

    public string? Sku { get; set; }

    public decimal? Price { get; set; }

    public int? StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsDefault { get; set; }

    public string? OptionGroupName1 { get; set; }

    public int? OptionValueId1 { get; set; }

    public string? OptionValueName1 { get; set; }

    public string? OptionGroupName2 { get; set; }

    public int? OptionValueId2 { get; set; }

    public string? OptionValueName2 { get; set; }
}
