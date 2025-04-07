using KDKMenShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Diagnostics;
using System.Globalization;
using X.PagedList;

namespace KDKMenShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ThoiTrangNamKDKContext _context;

        public HomeController(ILogger<HomeController> logger, ThoiTrangNamKDKContext context)
        {
            _logger = logger;
            _context = context;
        }
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            var sp = _context.SanPhams.Include(s => s.Ma.MaVoucherNavigation).AsNoTracking().ToList();
            // Lấy số lượng sản phẩm từ Session

            int? userId = HttpContext.Session.GetInt32("id");
            UpdateTTCN();


            return View(sp);
        }
        public void UpdateTTCN()
        {
            int? userId = HttpContext.Session.GetInt32("id");
            var gioHang = _context.GioHangs
                                .Where(g => g.Id == userId)
                                .Select(g => new { g.MaGioHang })
                                .FirstOrDefault();

            var wishlist = _context.Wishlists
                                 .Where(w => w.Id == userId)
                                 .Select(w => new { w.WishlistId })
                                 .FirstOrDefault();
            if (gioHang != null)
            {
                // Lấy số lượng các mục trong giỏ hàng từ cơ sở dữ liệu
                string cartId = gioHang.MaGioHang;
                int cartItemCount = (int)_context.ChiTietGioHangs
                                            .Where(g => g.MaGioHang == cartId)
                                            .Sum(g => g.SoLuong);

                ViewBag.CartItemCount = cartItemCount;
                HttpContext.Session.SetInt32("CartItemCount", cartItemCount);
                decimal cartItemSum = (decimal)_context.ChiTietGioHangs
                                            .Where(g => g.MaGioHang == cartId)
                                            .Sum(g => g.SoLuong * g.Gia);
                ViewBag.CartItemSum = cartItemSum;
                HttpContext.Session.SetInt32("CartItemSum", (int)cartItemSum);
            }
            else
            {
                    
                ViewBag.CartItemCount = 0;
                HttpContext.Session.SetInt32("CartItemCount", 0);
                ViewBag.CartItemSum = 0;
                HttpContext.Session.SetInt32("CartItemSum", 0);

            }

            if (wishlist != null)
            {
                // Lấy số lượng các mục trong giỏ hàng từ cơ sở dữ liệu
                int wishlistItemCount = _context.ChiTietWishlists
                                    .Where(g => g.WishlistId == wishlist.WishlistId)
                                    .Count();

                ViewBag.WishlistItemCount = wishlistItemCount;
                HttpContext.Session.SetInt32("WishlistItemCount", wishlistItemCount);
            }
            else
            {
                // Handle the case when gioHang is null, e.g., set cartItemCount to 0
                ViewBag.WishlistItemCount = 0;
                HttpContext.Session.SetInt32("WishlistItemCount", 0);
            }

            //Truyền số lượng mục vào ViewBag để sử dụng trong View

            ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
            ViewBag.ide = HttpContext.Session.GetInt32("id");
        }
        public IActionResult Privacy()
        {
            return View();
        }



        public IActionResult Error(int stastuscode)
        {
            ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
            if (stastuscode == 0)
            {
                return View("NotFound");
            }
            else
            {

            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        public IActionResult ChangeLanguage(string lang)
        {
            if (!string.IsNullOrEmpty(lang))
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("vi");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("vi");
                lang = "vi";
            }
            Response.Cookies.Append("Language", lang);
            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }
    }
}