using KDKMenShop.Models;
using KDKMenShop.Resrources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Http;
using System.Configuration;
using Google;
using Microsoft.Extensions.Options;
using KDKMenShop.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using KDKMenShop.Others;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Serilog;
using OfficeOpenXml;
using Microsoft.AspNetCore.Builder;
using Serilog.Events;


var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

//google
var googleClientId = configuration["Google:ClientId"];
var googleClientSecret = configuration["Google:ClientSecret"];

//facebook
var facebookAppId = configuration["Facebook:AppId"];
var facebookAppSecret = configuration["Google:AppSecret"];


//twitter
var twitterConsumerKey = configuration["Twitter:ConsumerKey"];
var twitterConsumerSecret = configuration["Twitter:ConsumerSecret"];

//
var twilloAccountSid = "AC4f1bf75d13cea7a05833a8b4f4994073";
var twilloAuthToken = "13dff3576308d68019a39a7d8cf9b842";
var twilloNumber = "+18788700248";


//email

var emailAdmin = configuration["Email:EmailAdmin"];

builder.Services.AddSingleton<ISmsService>(new TwilloSmsService(twilloAccountSid, twilloAuthToken, twilloNumber));

//
//var connectionString = builder.Configuration.GetConnectionString("ThoiTrangNamDKDConnection"); // Corrected connection string key
//builder.Services.AddDbContext<ThoiTrangNamKDKContext>(options => options.UseSqlServer(connectionString));
//builder.Services.AddDbContext<ThoiTrangNamKDKContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
//           .EnableSensitiveDataLogging()
//           .LogTo(message => LogQueryDetails(message), LogLevel.Information);
//});
var connectionString = builder.Configuration.GetConnectionString("ThoiTrangNamDKDConnection");
builder.Services.AddDbContext<ThoiTrangNamKDKContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging()
           .LogTo(message => LogQueryDetails(message), LogLevel.Information)
);

var listener = new QueryExecutionTimeListener();
builder.Services.AddSingleton(listener); // Singleton instance
DiagnosticListener.AllListeners.Subscribe(listener);

builder.Services.AddDbContext<ThoiTrangNamKDKContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ThoiTrangNamDKDConnection"))
           .EnableSensitiveDataLogging());

void LogQueryDetails(string message)
{
    if (message.Contains("Executed DbCommand")) // Filter for SQL command logs
    {
        Console.WriteLine($"Database query executed: {message}");
    }
}
builder.Services.AddScoped<ThoiTrangNamKDKContext>();
// nén file để giảm kích thước file và cải thiện thời gian tải.
builder.Services.AddResponseCompression(options =>
{
    //Thêm nén Brotli, chất lượng nén cao
    options.Providers.Add<BrotliCompressionProvider>();
    // Gzip dự phòng
    options.Providers.Add<GzipCompressionProvider>();
    // Bật nén cho các kết nối HTTPS
    options.EnableForHttps = true;
    // Định dạng các loại sẽ nén
    options.MimeTypes = new[] {
    "text/plain",
    "text/css",
    "application/javascript",
    "text/html",
    "application/xml",
    "application/json"
};
});
// nén file tốt nhất có thể
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal; // Or adjust based on testing
});


builder.Services.AddControllersWithViews();
// Thêm tái biên dịch runtime cho Razor


// Thêm dịch vụ xác thực Google vào IServiceCollection
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    
})
.AddCookie("Cookies")
.AddGoogle("Google", options =>
{
    options.ClientId = googleClientId;
    options.ClientSecret = googleClientSecret;
})
.AddFacebook("Facebook", options =>
{
    options.AppId = facebookAppId; // Lấy từ appsettings.json
    options.AppSecret = facebookAppSecret; // Lấy từ appsettings.json
})
.AddTwitter("Twitter", options =>
{
    options.ConsumerKey = twitterConsumerKey;
    options.ConsumerSecret = twitterConsumerSecret;
    options.CallbackPath = "/Account/TwitterCallback"; // Đường dẫn callback của bạn
});

// Thêm dịch vụ bộ nhớ cache
builder.Services.AddMemoryCache();
// Đăng ký DistributedCache (ví dụ dùng Redis hoặc SQL Server)
builder.Services.AddDistributedMemoryCache();
// Đăng ký CacheService
//builder.Services.AddTransient<CacheService>();
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(30);
    option.Cookie.IsEssential = true;
});

builder.Logging.AddConsole();
//Thêm log query vào service
builder.Services.AddScoped<LogQueryLocationFilter>();
// Register controllers with views and add the filter globally
builder.Services.AddControllersWithViews(options =>
{
	options.Filters.Add<LogQueryLocationFilter>();  // Apply the filter globally
});

// Configure Serilog
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.WriteTo.File("logs/log.txt")
	.CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddHostedService<ExcelRealTime>();
var app = builder.Build();

//app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");


// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//}



// Middleware để kiểm soát cache cho các tệp tĩnh
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
        ctx.Context.Response.Headers.Append("Pragma", "no-cache");
        ctx.Context.Response.Headers.Append("Expires", "0");
    }
});
// Middleware để kiểm soát cache cho tất cả các phản hồi
//app.Use(async (context, next) =>
//{
//    context.Response.Headers.Add("Cache-Control", "no-cache");
//    await next.Invoke();
//});
app.UseStaticFiles();
app.UseSession(); // Thêm Middleware Session vào pipeline
app.Use(async (context, next) =>
{
    string cookie = string.Empty;
    if (context.Request.Cookies.TryGetValue("Language", out cookie))
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cookie);
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(cookie);
    }
    else
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("vi");
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("vi");
    }
    await next.Invoke();
});






// Other middleware registrations
app.UseRouting();
app.UseAuthorization();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
  name: "quanlykichthuoc",
  pattern: "{area:exists}/quan-ly-kich-thuoc",
  defaults: new { controller = "Product", action = "DanhSachKichThuoc" });
    endpoints.MapControllerRoute(
  name: "quanlyDanhGia",
  pattern: "{area:exists}/quan-ly-danh-gia",
  defaults: new { controller = "Review", action = "DanhSachDanhGia" });
    endpoints.MapControllerRoute(
   name: "quanlykhuyenMai",
   pattern: "{area:exists}/quan-ly-khuyen-mai",
   defaults: new { controller = "Promotion", action = "KhuyenMai" });
    endpoints.MapControllerRoute(
    name: "quanlytaikhoanAdmin",
    pattern: "{area:exists}/quan-ly-tai-khoan",
    defaults: new { controller = "Account", action = "DanhSachTaiKhoan" });
    endpoints.MapControllerRoute(
    name: "quanlyDonhangAdmin",
    pattern: "{area:exists}/quan-ly-don-hang",
    defaults: new { controller = "Product", action = "DanhSachDonHang" });
    endpoints.MapControllerRoute(
    name: "productAdmin",
    pattern: "{area:exists}/quan-ly-bo-suu-tap",
    defaults: new { controller = "Collection", action = "Index" });
    endpoints.MapControllerRoute(
    name: "productAdmin",
    pattern: "{area:exists}/quan-ly-san-pham",
    defaults: new { controller = "Product", action = "Index" });

    endpoints.MapControllerRoute(
    name: "CategoryAdmin",
    pattern: "{area:exists}/quan-ly-danh-muc",
    defaults: new { controller = "Category", action = "Index" });

    endpoints.MapControllerRoute(
        name: "Areas",
        pattern: "{area:exists}/{controller=Home}/{action=Dasboard}/{id?}");




    endpoints.MapControllerRoute(
     name: "wishlist",
     pattern: "/danh-sach-yeu-thich/",
     defaults: new { Controller = "Wishlist", action = "Index" });
    endpoints.MapControllerRoute(
     name: "khuyenmai",
     pattern: "/khuyen-mai/",
     defaults: new { Controller = "Promotion", action = "Home" });
    endpoints.MapControllerRoute(
        name: "promotion",
        pattern: "/khuyen-mai/{Slug?}",
        defaults: new { Controller = "Promotion", action = "Index" });
    endpoints.MapControllerRoute(
      name: "lienhe",
      pattern: "/lien-he/",
      defaults: new { Controller = "Contact", action = "Index" });
   
    endpoints.MapControllerRoute(
      name: "dangky",
      pattern: "/dang-ky/",
      defaults: new { Controller = "Account", action = "DangKy" });
   
    endpoints.MapControllerRoute(
		name: "collection",
		pattern: "/loai-bo-suu-tap/{Slug?}",
		defaults: new { Controller = "Collection", action = "Index" });
	endpoints.MapControllerRoute(
	  name: "laylaimatkhau",
	  pattern: "/lay-lai-mat-khau/",
	  defaults: new { Controller = "Account", action = "LayLaiMatKhau" });
	endpoints.MapControllerRoute(
      name: "thongtincanhan",
      pattern: "/thong-tin-ca-nhan/",
      defaults: new { Controller = "Account", action = "ThongTinCaNhan" });
    endpoints.MapControllerRoute(
	  name: "order",
	  pattern: "/don-hang/",
	  defaults: new { Controller = "Order", action = "Index" });
	endpoints.MapControllerRoute(
        name: "category",
        pattern: "/loai-san-pham/{Slug?}",
        defaults: new{Controller = "Category", action= "Index"});
    endpoints.MapControllerRoute(
       name: "product",
       pattern: "/tat-ca-san-pham/",
       defaults: new { Controller = "Product", action = "Index" });
    endpoints.MapControllerRoute(
        name: "chitietsanpham",
        pattern: "/chi-tiet-san-pham/{maSP?}",
        defaults: new { Controller = "Product", action = "Detail" });

    endpoints.MapControllerRoute(
      name: "cart",
      pattern: "/gio-hang",
      defaults: new { Controller = "Cart", action = "Index" });
    endpoints.MapControllerRoute(
       name: "home",
       pattern: "/trang-chu/",
       defaults: new { Controller = "Home", action = "Index" });
    endpoints.MapControllerRoute(
       name: "category",
       pattern: "/bo-suu-tap/",
       defaults: new { Controller = "Collection", action = "Home" });
    endpoints.MapControllerRoute(
      name: "dangnhap",
      pattern: "/dang-nhap/",
      defaults: new { Controller = "Account", action = "DangNhap" });
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");


});

var serviceScope = app.Services.CreateScope();
var serviceProvider = serviceScope.ServiceProvider;


app.Run();
