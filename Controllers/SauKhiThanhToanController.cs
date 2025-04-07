using KDKMenShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KDKMenShop.Controllers
{
    public class SauKhiThanhToanController : Controller
    {
        private readonly ThoiTrangNamKDKContext _context;
        public SauKhiThanhToanController(ThoiTrangNamKDKContext context)
        {
            _context = context;
        }
        public IActionResult ThanhToanThanhCong()
        {
			ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
			ViewBag.ide = HttpContext.Session.GetInt32("id");
			int? userId = HttpContext.Session.GetInt32("id");
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            // Lấy tổng thời gian từ Session
            ViewBag.TotalTime = HttpContext.Session.GetInt32("TotalTime") ?? 0; // Gán mặc định là 0 nếu không có

            UpdateTTCN(userId);
            return View();
        }

        // GET: SauKhiThanhToan/ThanhToanThatBai
        public IActionResult ThanhToanThatBai()
        {
			ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
			ViewBag.ide = HttpContext.Session.GetInt32("id");

			int? userId = HttpContext.Session.GetInt32("id");
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            UpdateTTCN(userId);
            return View();
        }
        private void UpdateTTCN(int? userId)
        {
            if (userId == null)
                return;

            // Fetch `GioHang` and `Wishlist` information in a single query
            var userCartAndWishlist = _context.GioHangs
                .Where(g => g.Id == userId)
                .Select(g => new
                {
                    CartId = g.MaGioHang,
                    CartItemCount = (int ?)_context.ChiTietGioHangs
                                            .Where(c => c.MaGioHang == g.MaGioHang)
                                            .Sum(c => c.SoLuong),
                    CartItemSum = (decimal?)_context.ChiTietGioHangs
                                            .Where(c => c.MaGioHang == g.MaGioHang)
                                            .Sum(c => c.SoLuong * c.Gia),
                    WishlistId = _context.Wishlists
                                        .Where(w => w.Id == userId)
                                        .Select(w => w.WishlistId)
                                        .FirstOrDefault(),
                    WishlistItemCount = _context.ChiTietWishlists
                                                .Where(w => w.WishlistId == _context.Wishlists
                                                .Where(w => w.Id == userId)
                                                .Select(w => w.WishlistId)
                                                .FirstOrDefault())
                                                .Count()
                })
                .FirstOrDefault();

            // Update cart information
            if (userCartAndWishlist != null)
            {
                ViewBag.CartItemCount = userCartAndWishlist.CartItemCount ?? 0;
                HttpContext.Session.SetInt32("CartItemCount", userCartAndWishlist.CartItemCount ?? 0);

                ViewBag.CartItemSum = userCartAndWishlist.CartItemSum ?? 0;
                HttpContext.Session.SetInt32("CartItemSum", (int)(userCartAndWishlist.CartItemSum ?? 0));

                ViewBag.WishlistItemCount = userCartAndWishlist.WishlistItemCount;
                HttpContext.Session.SetInt32("WishlistItemCount", userCartAndWishlist.WishlistItemCount);
            }
            else
            {
                // Fallback if no cart/wishlist found
                ViewBag.CartItemCount = 0;
                HttpContext.Session.SetInt32("CartItemCount", 0);
                ViewBag.CartItemSum = 0;
                HttpContext.Session.SetInt32("CartItemSum", 0);
                ViewBag.WishlistItemCount = 0;
                HttpContext.Session.SetInt32("WishlistItemCount", 0);
            }
        }
    }
}
