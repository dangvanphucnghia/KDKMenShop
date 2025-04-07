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

// Google
var googleClientId = configuration["Google:ClientId"];
var googleClientSecret = configuration["Google:ClientSecret"];

// Facebook
var facebookAppId = configuration["Facebook:AppId"];
var facebookAppSecret = configuration["Facebook:AppSecret"];

// Twitter
var twitterConsumerKey = configuration["Twitter:ConsumerKey"];
var twitterConsumerSecret = configuration["Twitter:ConsumerSecret"];

// Twilio - dùng biến môi trường thay vì hardcode
var twilloAccountSid = Environment.GetEnvironmentVariable("TWILIO_SID") ?? "fake_sid";
var twilloAuthToken = Environment.GetEnvironmentVariable("TWILIO_TOKEN") ?? "fake_token";
var twilloNumber = Environment.GetEnvironmentVariable("TWILIO_NUMBER") ?? "+10000000000";

// Email
var emailAdmin = configuration["Email:EmailAdmin"];

builder.Services.AddSingleton<ISmsService>(new TwilloSmsService(twilloAccountSid, twilloAuthToken, twilloNumber));

var connectionString = builder.Configuration.GetConnectionString("ThoiTrangNamDKDConnection");
builder.Services.AddDbContext<ThoiTrangNamKDKContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging()
           .LogTo(message => LogQueryDetails(message), LogLevel.Information)
);

var listener = new QueryExecutionTimeListener();
builder.Services.AddSingleton(listener);
DiagnosticListener.AllListeners.Subscribe(listener);

void LogQueryDetails(string message)
{
    if (message.Contains("Executed DbCommand"))
    {
        Console.WriteLine($"Database query executed: {message}");
    }
}
builder.Services.AddScoped<ThoiTrangNamKDKContext>();

builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.EnableForHttps = true;
    options.MimeTypes = new[] {
        "text/plain", "text/css", "application/javascript",
        "text/html", "application/xml", "application/json"
    };
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.AddControllersWithViews();
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
    options.AppId = facebookAppId;
    options.AppSecret = facebookAppSecret;
})
.AddTwitter("Twitter", options =>
{
    options.ConsumerKey = twitterConsumerKey;
    options.ConsumerSecret = twitterConsumerSecret;
    options.CallbackPath = "/Account/TwitterCallback";
});

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(30);
    option.Cookie.IsEssential = true;
});

builder.Logging.AddConsole();
builder.Services.AddScoped<LogQueryLocationFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<LogQueryLocationFilter>();
});

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/log.txt")
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddHostedService<ExcelRealTime>();

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
        ctx.Context.Response.Headers.Append("Pragma", "no-cache");
        ctx.Context.Response.Headers.Append("Expires", "0");
    }
});
app.UseStaticFiles();
app.UseSession();
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

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

var serviceScope = app.Services.CreateScope();
var serviceProvider = serviceScope.ServiceProvider;

app.Run();
