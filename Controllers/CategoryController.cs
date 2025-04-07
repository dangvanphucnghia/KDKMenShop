using KDKMenShop.Models;
using KDKMenShop.Models.ProductModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using X.PagedList;

namespace KDKMenShop.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ThoiTrangNamKDKContext _context;
        public CategoryController(ThoiTrangNamKDKContext context)
        {
            _context = context;
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index( int? page, string Slug="")
        {
			const int pageSize = 12; // Number of items per page
			var sp = _context.SanPhams.ToList();
            // Lấy số lượng sản phẩm từ Session

            int? userId = HttpContext.Session.GetInt32("id");

            UpdateTTCN();


            LoaiSp loaiSp = _context.LoaiSps.Where(a => a.Slug == Slug).FirstOrDefault();
            if (loaiSp == null)
            {
                return RedirectToAction("Index");  
            }
			ViewBag.Slug = Slug;


            var product = _context.SanPhams.Where(a => a.MaLoaiSp == loaiSp.MaLoaiSp).Include(s => s.Ma.MaVoucherNavigation).OrderByDescending(s => s.MaSanPham);
			var pageNumber = page ?? 1; //mặc định 1
			var pagedProducts = await product.ToPagedListAsync(pageNumber, pageSize);
			return View(pagedProducts);
        }
        public void UpdateTTCN()
        {
            int? userId = HttpContext.Session.GetInt32("id");

            var gioHang = _context.GioHangs.FirstOrDefault(g => g.Id == userId);
            var wishlist = _context.Wishlists.FirstOrDefault(w => w.Id == userId);
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
            //Truyền số lượng mục vào ViewBag để sử dụng trong View

            ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
            ViewBag.ide = HttpContext.Session.GetInt32("id");
        }
       

    }
}
