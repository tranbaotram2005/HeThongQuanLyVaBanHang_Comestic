using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class Supplier
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? ContactName { get; set; }

    public string? ContactTitle { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? HomePage { get; set; }

    public int? AddressId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Notes { get; set; }

    public int Status { get; set; }

    public virtual Address? Address { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual User? UpdatedByNavigation { get; set; }
}
