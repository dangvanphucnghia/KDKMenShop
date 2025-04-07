using KDKMenShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KDKMenShop.Controllers
{
    public class FooterController : Controller
    {
		private readonly ThoiTrangNamKDKContext _context;
		public FooterController(ThoiTrangNamKDKContext context)
		{
			_context = context;
		}

        public IActionResult GioiThieu()
        {
			UpdateCartInfo();
            return View();
        }

        public IActionResult ChinhSachBanHang()
        {
			UpdateCartInfo();
			return View();
        }

        public IActionResult ChinhSachDoiTra()
        {
			UpdateCartInfo();
			return View();
        }

        public IActionResult ChinhSachKhuyenMai()
        {
			UpdateCartInfo();
			return View();
        }

        public IActionResult DieuKhoanMuaBan()
        {
			UpdateCartInfo();
			return View();
        }
		public void UpdateCartInfo()
		{
			int? userId = HttpContext.Session.GetInt32("id");
			var maGioHang = _context.GioHangs
							   .Where(gh => gh.Id == userId)
							   .Select(gh => gh.MaGioHang)
							   .FirstOrDefault();

			string taiKhoan = HttpContext.Session.GetString("TaiKhoan");
			ViewBag.cc = taiKhoan;

			int cartItemCount = (int)_context.ChiTietGioHangs
										.Where(g => g.MaGioHang == maGioHang)
										.Sum(g => g.SoLuong);

			ViewBag.CartItemCount = cartItemCount;
			HttpContext.Session.SetInt32("CartItemCount", cartItemCount);

			var wishlist = _context.Wishlists.FirstOrDefault(w => w.Id == userId);
			if (wishlist != null)
			{
				string wishlistID = wishlist.WishlistId;
				int wishlistItemCount = _context.ChiTietWishlists
												.Where(g => g.WishlistId == wishlistID)
												.Count();

				ViewBag.WishlistItemCount = wishlistItemCount;
				HttpContext.Session.SetInt32("WishlistItemCount", wishlistItemCount);
			}

			decimal cartItemSum = (decimal)_context.ChiTietGioHangs
												.Where(g => g.MaGioHang == maGioHang)
												.Sum(g => g.SoLuong * g.Gia);
			ViewBag.CartItemSum = cartItemSum;
			HttpContext.Session.SetInt32("CartItemSum", (int)cartItemSum);
			if (wishlist != null)
			{
				// Lấy số lượng các mục trong giỏ hàng từ cơ sở dữ liệu
				string wishlistID = wishlist.WishlistId;
				int wishlistItemCount = _context.ChiTietWishlists
									.Where(g => g.WishlistId == wishlistID).Count();

				ViewBag.WishlistItemCount = wishlistItemCount;
				HttpContext.Session.SetInt32("WishlistItemCount", wishlistItemCount);
			}
			else
			{
				// Handle the case when gioHang is null, e.g., set cartItemCount to 0
				ViewBag.WishlistItemCount = 0;
				HttpContext.Session.SetInt32("WishlistItemCount", 0);

			}
		}
	}
}
