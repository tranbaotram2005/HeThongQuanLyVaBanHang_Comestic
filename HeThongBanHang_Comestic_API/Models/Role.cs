using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class Role
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? NormalizedName { get; set; }

    public string Description { get; set; } = null!;

    public string? ConcurrencyStamp { get; set; }

    public virtual ICollection<AspNetRoleClaim> AspNetRoleClaims { get; set; } = new List<AspNetRoleClaim>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
