using KDKMenShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace KDKMenShop.Controllers
{
    public class ErrorController : Controller
    {
		private readonly ThoiTrangNamKDKContext _data;
		public ErrorController(ThoiTrangNamKDKContext context)
		{
			_data = context;
		}
        public IActionResult Index()
        {
			int? userId = HttpContext.Session.GetInt32("id");
			ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
			ViewBag.ide = HttpContext.Session.GetInt32("id");
			UpdateTTCN();
			return View();
        }
		public IActionResult ChucNangNotDone()
		{
			int? userId = HttpContext.Session.GetInt32("id");
			ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
			ViewBag.ide = HttpContext.Session.GetInt32("id");
			UpdateTTCN();
			return View();
		}
		public void UpdateTTCN()
		{
			int? userId = HttpContext.Session.GetInt32("id");
			var gioHang = _data.GioHangs.FirstOrDefault(g => g.Id == userId);
			var wishlist = _data.Wishlists.FirstOrDefault(w => w.Id == userId);
			if (gioHang != null)
			{
				// Lấy số lượng các mục trong giỏ hàng từ cơ sở dữ liệu
				string cartId = gioHang.MaGioHang;
				int cartItemCount = (int)_data.ChiTietGioHangs
											.Where(g => g.MaGioHang == cartId)
											.Sum(g => g.SoLuong);

				ViewBag.CartItemCount = cartItemCount;
				HttpContext.Session.SetInt32("CartItemCount", cartItemCount);
				decimal cartItemSum = (decimal)_data.ChiTietGioHangs
											.Where(g => g.MaGioHang == cartId)
											.Sum(g => g.SoLuong * g.Gia);
				ViewBag.CartItemSum = cartItemSum;
				HttpContext.Session.SetInt32("CartItemSum", (int)cartItemSum);
			}
			else
			{
				// Handle the case when gioHang is null, e.g., set cartItemCount to 0
				ViewBag.CartItemCount = 0;
				HttpContext.Session.SetInt32("CartItemCount", 0);
				ViewBag.CartItemSum = 0;
				HttpContext.Session.SetInt32("CartItemSum", 0);

			}
			if (wishlist != null)
			{
				// Lấy số lượng các mục trong giỏ hàng từ cơ sở dữ liệu
				string wishlistID = wishlist.WishlistId;
				int wishlistItemCount = _data.ChiTietWishlists
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

			//Truyền số lượng mục vào ViewBag để sử dụng trong View

			ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
			ViewBag.ide = HttpContext.Session.GetInt32("id");
		}
	}
}
