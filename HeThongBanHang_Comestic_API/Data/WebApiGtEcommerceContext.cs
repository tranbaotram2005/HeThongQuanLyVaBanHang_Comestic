using System;
using System.Collections.Generic;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Data;

public partial class WebApiGtEcommerceContext : DbContext
{
    public WebApiGtEcommerceContext()
    {
    }

    public WebApiGtEcommerceContext(DbContextOptions<WebApiGtEcommerceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductOption> ProductOptions { get; set; }

    public virtual DbSet<ProductOptionValue> ProductOptionValues { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<ProductVariantOptionValue> ProductVariantOptionValues { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ShipInfo> ShipInfos { get; set; }

    public virtual DbSet<Shipper> Shippers { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<ViewProductVariantDetail> ViewProductVariantDetails { get; set; }

    public virtual DbSet<ViewProductVariantFullDetail> ViewProductVariantFullDetails { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost; Initial Catalog=WebAPI_GT_Ecommerce; Persist Security Info=True; User ID=sa; Password=123456; Trust Server Certificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Addresses");

            entity.ToTable("Address");

            entity.Property(e => e.AddressNo).HasMaxLength(150);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Extension).HasMaxLength(15);
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.PostalCode).HasMaxLength(15);
            entity.Property(e => e.Region).HasMaxLength(15);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AddressCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Address_Users_Created");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.AddressUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Address_Users_Updated");
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_RoleClaims_Roles");
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserClaims_Users");
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserLogins_Users");
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserTokens_Users");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("Cart");

            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Cart_Users");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItem");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK_CartItem_Cart");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductVariantId)
                .HasConstraintName("FK_CartItem_ProductVariant");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Categories");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryName).HasMaxLength(150);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CategoryCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Category_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.CategoryUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Category_UpdatedBy");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Customers");

            entity.ToTable("Customer");

            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.ContactName).HasMaxLength(30);
            entity.Property(e => e.ContactTitle).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Address).WithMany(p => p.Customers)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK_Customer_Address");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CustomerCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Customer_Users_Created");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.CustomerUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Customer_Users_Updated");

            entity.HasOne(d => d.User).WithMany(p => p.CustomerUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Customer_Users");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Employees");

            entity.ToTable("Employee");

            entity.Property(e => e.BirthDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.HireDate).HasColumnType("datetime");
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(250);
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Photo).HasColumnType("image");
            entity.Property(e => e.PhotoPath).HasMaxLength(255);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.TitleOfCourtesy).HasMaxLength(25);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Address).WithMany(p => p.Employees)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK_Employee_Address");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.EmployeeCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Employee_Users_Created");

            entity.HasOne(d => d.ReportsToNavigation).WithMany(p => p.InverseReportsToNavigation).HasForeignKey(d => d.ReportsTo);

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.EmployeeUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Employee_Users_Updated");

            entity.HasOne(d => d.User).WithMany(p => p.EmployeeUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employee_Users");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("Inventory");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.Quantity).HasDefaultValue(0);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken()
                .HasColumnName("row_version");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Inventory_Users_Created");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.InventoryUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Inventory_Users_Updated");

            entity.HasOne(d => d.Variant).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inventory_ProductVariant");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Notifications_Order");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Orders");

            entity.ToTable("Order");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Freight)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.Receive).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.OrderCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Order_Users_Created");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Order_Customer");

            entity.HasOne(d => d.Employee).WithMany(p => p.Orders)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_Order_Employee");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.OrderUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Order_Users_Updated");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_OrderDetails");

            entity.ToTable("OrderDetail");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LineTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.Quantity).HasDefaultValue((short)1);
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.VariantInfo).HasMaxLength(512);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.OrderDetailCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_OrderDetail_Users_Created");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetail_Order");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetail_Product");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductVariantId)
                .HasConstraintName("FK_OrderDetail_ProductVariant");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.OrderDetailUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_OrderDetail_Users_Updated");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Payments");

            entity.ToTable("Payment");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Method).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PaymentCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Payment_Users_Created");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Order");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.PaymentUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Payment_Users_Updated");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");

            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.QuantityPerUnit).HasMaxLength(20);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UnitPrice)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Product_Category");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProductCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Product_CreatedBy");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Products)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK_Product_Supplier");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ProductUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Product_UpdatedBy");
        });

        modelBuilder.Entity<ProductOption>(entity =>
        {
            entity.ToTable("ProductOption");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProductOptionCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_ProductOption_Users_Created");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductOptions)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductOption_Product");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ProductOptionUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_ProductOption_Users_Updated");
        });

        modelBuilder.Entity<ProductOptionValue>(entity =>
        {
            entity.ToTable("ProductOptionValue");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProductOptionValueCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_ProductOptionValue_Users_Created");

            entity.HasOne(d => d.ProductOption).WithMany(p => p.ProductOptionValues)
                .HasForeignKey(d => d.ProductOptionId)
                .HasConstraintName("FK_ProductOptionValue_Option");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ProductOptionValueUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_ProductOptionValue_Users_Updated");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("ProductVariant");

            entity.HasIndex(e => e.Sku, "UQ_ProductVariant_SKU").IsUnique();

            entity.Property(e => e.Barcode).HasMaxLength(50);
            entity.Property(e => e.CompareAtPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Height).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.Length).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RowVersions)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Weight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Width).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProductVariantCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_ProductVariant_Users_Created");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ProductVariant_Product");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ProductVariantUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_ProductVariant_Users_Updated");
        });

        modelBuilder.Entity<ProductVariantOptionValue>(entity =>
        {
            entity.ToTable("ProductVariantOptionValue");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.ProductOptionValue).WithMany(p => p.ProductVariantOptionValues)
                .HasForeignKey(d => d.ProductOptionValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PVov_Value");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.ProductVariantOptionValues)
                .HasForeignKey(d => d.ProductVariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PVov_Variant");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<ShipInfo>(entity =>
        {
            entity.ToTable("ShipInfo");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.RequiredDate).HasColumnType("datetime");
            entity.Property(e => e.ShipAddress).HasMaxLength(150);
            entity.Property(e => e.ShipCity).HasMaxLength(50);
            entity.Property(e => e.ShipCountry).HasMaxLength(50);
            entity.Property(e => e.ShipName).HasMaxLength(50);
            entity.Property(e => e.ShipPhone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.ShipPostalCode).HasMaxLength(15);
            entity.Property(e => e.ShipRegion).HasMaxLength(15);
            entity.Property(e => e.ShippedDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Address).WithMany(p => p.ShipInfos)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK_ShipInfo_Address");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ShipInfoCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_ShipInfo_Users_Created");

            entity.HasOne(d => d.Order).WithMany(p => p.ShipInfos)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShipInfo_Order");

            entity.HasOne(d => d.Shipper).WithMany(p => p.ShipInfos)
                .HasForeignKey(d => d.ShipperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShipInfo_Shipper");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ShipInfoUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_ShipInfo_Users_Updated");
        });

        modelBuilder.Entity<Shipper>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Shippers");

            entity.ToTable("Shipper");

            entity.Property(e => e.CompanyName).HasMaxLength(40);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(24);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Address).WithMany(p => p.Shippers)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK_Shipper_Address");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ShipperCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Shipper_Users_Created");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ShipperUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Shipper_Users_Updated");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Suppliers");

            entity.ToTable("Supplier");

            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.ContactName).HasMaxLength(100);
            entity.Property(e => e.ContactTitle).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HomePage).HasMaxLength(250);
            entity.Property(e => e.Notes).HasMaxLength(256);
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Address).WithMany(p => p.Suppliers)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK_Supplier_Address");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.SupplierCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Supplier_Users_Created");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.SupplierUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Supplier_Users_Updated");
            entity.Property(e => e.Picture).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK_AspNetUserRoles_Roles"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_AspNetUserRoles_Users"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                    });
        });

        modelBuilder.Entity<ViewProductVariantDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("viewProductVariantDetail");

            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
        });

        modelBuilder.Entity<ViewProductVariantFullDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("viewProductVariantFullDetail");

            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.OptionGroupName1).HasMaxLength(100);
            entity.Property(e => e.OptionGroupName2).HasMaxLength(100);
            entity.Property(e => e.OptionValueName1).HasMaxLength(100);
            entity.Property(e => e.OptionValueName2).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.ToTable("Wishlist");

            entity.HasIndex(e => new { e.UserId, e.ProductId }, "UQ_Wishlist_User_Product").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Product).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Wishlist_Product");

            entity.HasOne(d => d.User).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Wishlist_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
