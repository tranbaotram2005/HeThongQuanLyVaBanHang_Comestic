using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class Cart
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? CouponCode { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User User { get; set; } = null!;
}
