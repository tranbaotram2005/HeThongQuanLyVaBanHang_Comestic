using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class AspNetRoleClaim
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }

    public virtual Role Role { get; set; } = null!;
}
