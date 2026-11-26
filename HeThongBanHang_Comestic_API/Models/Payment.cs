using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public string? Method { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Notes { get; set; }

    public int? Status { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User? UpdatedByNavigation { get; set; }
}
