using System;
using System.Collections.Generic;

namespace HeThongBanHang_Comestic_API.Models;

public partial class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public virtual ICollection<Address> AddressCreatedByNavigations { get; set; } = new List<Address>();

    public virtual ICollection<Address> AddressUpdatedByNavigations { get; set; } = new List<Address>();

    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();

    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();

    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Category> CategoryCreatedByNavigations { get; set; } = new List<Category>();

    public virtual ICollection<Category> CategoryUpdatedByNavigations { get; set; } = new List<Category>();

    public virtual ICollection<Customer> CustomerCreatedByNavigations { get; set; } = new List<Customer>();

    public virtual ICollection<Customer> CustomerUpdatedByNavigations { get; set; } = new List<Customer>();

    public virtual ICollection<Customer> CustomerUsers { get; set; } = new List<Customer>();

    public virtual ICollection<Employee> EmployeeCreatedByNavigations { get; set; } = new List<Employee>();

    public virtual ICollection<Employee> EmployeeUpdatedByNavigations { get; set; } = new List<Employee>();

    public virtual ICollection<Employee> EmployeeUsers { get; set; } = new List<Employee>();

    public virtual ICollection<Inventory> InventoryCreatedByNavigations { get; set; } = new List<Inventory>();

    public virtual ICollection<Inventory> InventoryUpdatedByNavigations { get; set; } = new List<Inventory>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> OrderCreatedByNavigations { get; set; } = new List<Order>();

    public virtual ICollection<OrderDetail> OrderDetailCreatedByNavigations { get; set; } = new List<OrderDetail>();

    public virtual ICollection<OrderDetail> OrderDetailUpdatedByNavigations { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Order> OrderUpdatedByNavigations { get; set; } = new List<Order>();

    public virtual ICollection<Payment> PaymentCreatedByNavigations { get; set; } = new List<Payment>();

    public virtual ICollection<Payment> PaymentUpdatedByNavigations { get; set; } = new List<Payment>();

    public virtual ICollection<Product> ProductCreatedByNavigations { get; set; } = new List<Product>();

    public virtual ICollection<ProductOption> ProductOptionCreatedByNavigations { get; set; } = new List<ProductOption>();

    public virtual ICollection<ProductOption> ProductOptionUpdatedByNavigations { get; set; } = new List<ProductOption>();

    public virtual ICollection<ProductOptionValue> ProductOptionValueCreatedByNavigations { get; set; } = new List<ProductOptionValue>();

    public virtual ICollection<ProductOptionValue> ProductOptionValueUpdatedByNavigations { get; set; } = new List<ProductOptionValue>();

    public virtual ICollection<Product> ProductUpdatedByNavigations { get; set; } = new List<Product>();

    public virtual ICollection<ProductVariant> ProductVariantCreatedByNavigations { get; set; } = new List<ProductVariant>();

    public virtual ICollection<ProductVariant> ProductVariantUpdatedByNavigations { get; set; } = new List<ProductVariant>();

    public virtual ICollection<ShipInfo> ShipInfoCreatedByNavigations { get; set; } = new List<ShipInfo>();

    public virtual ICollection<ShipInfo> ShipInfoUpdatedByNavigations { get; set; } = new List<ShipInfo>();

    public virtual ICollection<Shipper> ShipperCreatedByNavigations { get; set; } = new List<Shipper>();

    public virtual ICollection<Shipper> ShipperUpdatedByNavigations { get; set; } = new List<Shipper>();

    public virtual ICollection<Supplier> SupplierCreatedByNavigations { get; set; } = new List<Supplier>();

    public virtual ICollection<Supplier> SupplierUpdatedByNavigations { get; set; } = new List<Supplier>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
