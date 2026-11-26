using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class Shipper
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? Phone { get; set; }

    public int? AddressId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Notes { get; set; }

    public int? Status { get; set; }

    public virtual Address? Address { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<ShipInfo> ShipInfos { get; set; } = new List<ShipInfo>();

    public virtual User? UpdatedByNavigation { get; set; }
}
