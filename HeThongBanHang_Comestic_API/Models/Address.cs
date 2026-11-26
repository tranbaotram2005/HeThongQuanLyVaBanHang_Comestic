using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class Address
{
    public int Id { get; set; }

    public string? AddressNo { get; set; }

    public int? CountryId { get; set; }

    public int? ProvinceId { get; set; }

    public int? WardId { get; set; }

    public int? TownId { get; set; }

    public string? City { get; set; }

    public string? Region { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? Extension { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Notes { get; set; }

    public int? Status { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<ShipInfo> ShipInfos { get; set; } = new List<ShipInfo>();

    public virtual ICollection<Shipper> Shippers { get; set; } = new List<Shipper>();

    public virtual ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();

    public virtual User? UpdatedByNavigation { get; set; }
}
