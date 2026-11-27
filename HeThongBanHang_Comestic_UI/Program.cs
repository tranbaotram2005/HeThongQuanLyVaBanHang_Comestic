using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

//Cho phép đọc HttpContext (Session/User) trong các class Service
builder.Services.AddHttpContextAccessor();


// Đăng ký Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//Bật Cookie Auth của MVC để @User.Identity hoạt động ở View
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(180);
        options.SlidingExpiration = false;
    });

builder.Services.AddTransient<AuthHeaderHandler>();

//Lấy BaseUrl từ appsettings.json và đăng ký HttpClient cho ApiService
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("Thiếu ApiSettings:BaseUrl trong appsettings.json");
builder.Services.AddHttpClient<ICategoryApiService, CategoryApiService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<IProductApiService, ProductApiService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<IProductVariantApiService, ProductVariantApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IAccountApiService, AccountApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ICartApiService, CartApiService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<IWishlistApiService, WishlistApiService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<ISupplierApiService, SupplierApiService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<IOrderApiService, OrderApiService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<IPaymentApiService, PaymentApiService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();        // Phải đặt trước Authentication/MapControllerRoute
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
