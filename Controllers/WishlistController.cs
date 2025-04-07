using KDKMenShop.Models;
using KDKMenShop.Models.Model;
using KDKMenShop.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Xml.Linq;

public class WishlistController : Controller
{
    private readonly ThoiTrangNamKDKContext _context; // Your DbContext class

    public WishlistController(ThoiTrangNamKDKContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        int? userId = HttpContext.Session.GetInt32("id");

        // Kiểm tra xem người dùng đã đăng nhập chưa
        if (userId == null || string.IsNullOrEmpty(HttpContext.Session.GetString("TaiKhoan")))
        {
            return RedirectToAction("DangNhap", "Account");
        }

        // Get the wishlist and cart details in a single query
        var userData = (from w in _context.Wishlists
                        where w.Id == userId
                        let cart = _context.GioHangs.FirstOrDefault(g => g.Id == userId)
                        select new
                        {
                            WishlistId = w.WishlistId,
                            CartId = cart != null ? cart.MaGioHang : null,
                            CartItemCount = cart != null
                                ? _context.ChiTietGioHangs.Where(c => c.MaGioHang == cart.MaGioHang).Sum(c => (int?)c.SoLuong) ?? 0
                                : 0,
                            CartItemSum = cart != null
                                ? _context.ChiTietGioHangs.Where(c => c.MaGioHang == cart.MaGioHang).Sum(c => (decimal?)(c.SoLuong * c.Gia)) ?? 0
                                : 0,
                            WishlistItemCount = _context.ChiTietWishlists.Count(g => g.WishlistId == w.WishlistId)
                        }).FirstOrDefault();

        if (userData == null)
        {
            return RedirectToAction("DangNhap", "Account");
        }

        ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
        ViewBag.CartItemCount = userData.CartItemCount;
        ViewBag.CartItemSum = userData.CartItemSum;
        HttpContext.Session.SetInt32("CartItemCount", userData.CartItemCount);
        HttpContext.Session.SetInt32("CartItemSum", (int)userData.CartItemSum);
        ViewBag.WishlistItemCount = userData.WishlistItemCount;
        HttpContext.Session.SetInt32("WishlistItemCount", userData.WishlistItemCount);

        // Fetch wishlist items with products and discounts in one query
        var wishlistItems = (from w in _context.ChiTietWishlists
                             join sp in _context.SanPhams on w.MaSanPham equals sp.MaSanPham
                             join km in _context.ChiTietKhuyenMais
                                 on sp.MaSanPham equals km.MaSanPham into discountGroup
                             from discount in discountGroup
                                 .Where(d => d.MaVoucherNavigation.ThoiGianBatDau <= DateTime.Now &&
                                             d.MaVoucherNavigation.ThoiGianKetThuc >= DateTime.Now)
                                 .DefaultIfEmpty()
                             where w.WishlistId == userData.WishlistId
                             select new WishlistItemModel
                             {
                                 ProductId = sp.MaSanPham,
                                 TenSP = sp.TenSanPham,
                                 Gia = sp.Gia,
                                 GiaSale = discount != null ? (int)(sp.Gia - (sp.Gia * discount.MaVoucherNavigation.PhanTramGiam / 100)) : sp.Gia,
                                 image = sp.HinhAnh
                             }).ToList();

        var wishViewModel = new WishlistViewModel
        {
            WishlistItems = wishlistItems,
        };

        return View(wishViewModel);
    }
    private string GenerateUniqueWishlistCode(int? userId)
	{
		// Tạo mã giỏ hàng từ userId và thời gian hiện tại
		string uniqueCode = $"{userId}-{DateTime.Now.Ticks}";

		// Chuyển mã thành hash code (băm)
		int hashCode = uniqueCode.GetHashCode();

		// Chuyển hash code thành số dương để tránh số âm
		hashCode = hashCode & 0x7FFFFFFF;

		// Lấy 5 chữ số cuối cùng của hash code
		string cartCode = hashCode.ToString().Substring(Math.Max(0, hashCode.ToString().Length - 5));

		// Trả về mã giỏ hàng duy nhất
		return cartCode;
	}

	public async Task<IActionResult> AddToWishlist(int MaSanPham)
	{
		int? userId = HttpContext.Session.GetInt32("id");
		// Kiểm tra xem người dùng đã đăng nhập chưa
		if (userId == null || string.IsNullOrEmpty(HttpContext.Session.GetString("TaiKhoan")))
		{
			//return RedirectToAction("DangNhap", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
			return Json(new { success = false, error = "Vui lòng đăng nhập để thêm sản phẩm vào danh sách yêu thích" });
		}

		var sanpham = _context.SanPhams.FirstOrDefault(p => p.MaSanPham == MaSanPham);
		if (sanpham == null)
		{
			return NotFound();
		}

        var existingWishlist = await _context.Wishlists.FirstOrDefaultAsync(w => w.Id == userId);
        string wishlistId = existingWishlist?.WishlistId ?? GenerateUniqueWishlistCode(userId);

        if (existingWishlist == null)
        {
            // Add new wishlist if it doesn't exist
            _context.Wishlists.Add(new Wishlist { WishlistId = wishlistId, Id = (int)userId });
            await _context.SaveChangesAsync();
        }

        // Check if product is already in the wishlist
        bool alreadyExists = await _context.ChiTietWishlists.AnyAsync(c => c.WishlistId == wishlistId && c.MaSanPham == MaSanPham);
        if (!alreadyExists)
        {
            _context.ChiTietWishlists.Add(new ChiTietWishlist { WishlistId = wishlistId, MaSanPham = MaSanPham });
            await _context.SaveChangesAsync();
        }

        int wishlistItemCount = await _context.ChiTietWishlists.CountAsync(g => g.WishlistId == wishlistId);
        return Json(new { success = true, wishlistItemCount, message = "Thêm sản phẩm vào danh sách yêu thích thành công." });
    }

	[HttpPost]
    public IActionResult DeleteFromWishlist(int MaSanPham)
    {
        try
        {
            int? userId = HttpContext.Session.GetInt32("id");

            string wishlistId = _context.Wishlists
                .FirstOrDefault(w => w.Id == userId)?.WishlistId;

            var chiTiet = _context.ChiTietWishlists
                .FirstOrDefault(c => c.WishlistId == wishlistId && c.MaSanPham == MaSanPham);

            if (chiTiet != null)
            {
                _context.ChiTietWishlists.Remove(chiTiet);
                _context.SaveChanges();
                int wishlistItemCount = _context.ChiTietWishlists
                                        .Where(g => g.WishlistId == wishlistId).Count();
                return Json(new { success = true, wishlistItemCount, message = "Xóa sản phẩm khỏi Wishlist thành công." });
            }
            else
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong danh sách yêu thích." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Đã có lỗi khi xóa sản phẩm. " });
        }
    }
}