using KDKMenShop.Models;
using KDKMenShop.Models.ProductModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using X.PagedList;

namespace KDKMenShop.Controllers
{
	public class CollectionController : Controller
	{
		private readonly ThoiTrangNamKDKContext _context;
		public CollectionController(ThoiTrangNamKDKContext context)
		{
			_context = context;
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
		public async Task<IActionResult> Index(int? page, string Slug = "", string category = "", decimal? minPrice = null, decimal? maxPrice = null)
		{
            const int pageSize = 4; // Number of items per page
            var sp = _context.SanPhams.ToList();
            // Lấy số lượng sản phẩm từ Session
            UpdateTTCN();
            BoSuuTap boSuuTap = _context.BoSuuTaps.Where(s => s.Slug == Slug).FirstOrDefault();
			if (boSuuTap == null)
			{
				return RedirectToAction("Index");
			}
            var categories = _context.LoaiSps.Select(c => c.TenLoaiSp).ToList();
            ViewBag.Categories = categories;
            ViewBag.Slug = Slug;
			var products = _context.SanPhams
                          .Where(s => s.MaBoSuuTap == boSuuTap.MaBoSuuTap).Include(s => s.Ma.MaVoucherNavigation)
                          .OrderByDescending(s => s.MaSanPham);


            // Apply additional filters based on parameters
            if (!string.IsNullOrEmpty(category))
            {
                products = (IOrderedQueryable<SanPham>)products.Where(p => p.MaLoaiSpNavigation.TenLoaiSp == category);
            }
            if (minPrice.HasValue)
            {
                products = (IOrderedQueryable<SanPham>)products.Where(p => p.Gia >= minPrice);
            }
            if (maxPrice.HasValue)
            {
                products = (IOrderedQueryable<SanPham>)products.Where(p => p.Gia <= maxPrice);
            }
            var pageNumber = page ?? 1;  //mặc định 1
			var pagedProducts = await products.ToPagedListAsync(pageNumber, pageSize);
            return View(pagedProducts);
		}
        public IActionResult Home()
        {
            var sp = _context.SanPhams.Include(s => s.Ma.MaVoucherNavigation).ToList();
            // Lấy số lượng sản phẩm từ Session

            int? userId = HttpContext.Session.GetInt32("id");

            UpdateTTCN();
            return View(sp);
        }
		[HttpGet]
		public IActionResult TimKiemSanPhamBoSuuTap(string q)
		{
			var danhSachSanPhamBoSuuTap = _context.SanPhams
	                                        .Where(sp =>  sp.MaBoSuuTap != 0 && (sp.TenSanPham.StartsWith(q) || sp.TenSanPham.Contains(q)))
	                                        .Select(sp => sp.TenSanPham)
	                                        .Distinct()
	                                        .Take(6)
	                                        .ToList();

			return Json(danhSachSanPhamBoSuuTap);
		}
		public IActionResult SearchCollection(string searchString)
		{
			if (string.IsNullOrWhiteSpace(searchString))
			{
				searchString = "";
			}
			else
			{
				searchString = searchString.Trim();
			}

			var products = _context.SanPhams
							.Where(p => p.MaBoSuuTap != 0  && p.TenSanPham.Contains(searchString))
							.ToList();
			int? userId = HttpContext.Session.GetInt32("id");
			UpdateTTCN();
			return View(products);
		}
		public async Task<IActionResult> FilterProducts(string category, decimal? minPrice, decimal? maxPrice, int? page, string slug)
		{
			const int pageSize = 4; // Number of items per page

			BoSuuTap bst = _context.BoSuuTaps.FirstOrDefault(s => s.Slug == slug); // Get the collection based on the Slug
			var products = _context.SanPhams.AsQueryable();

			products = products.Where(p =>
				(p.MaBoSuuTap == bst.MaBoSuuTap) &&
				(string.IsNullOrEmpty(category) || p.MaLoaiSpNavigation.TenLoaiSp == category) &&
				(!minPrice.HasValue || p.Gia >= minPrice) &&
				(!maxPrice.HasValue || p.Gia <= maxPrice)
			);

			// Paginate the filtered products
			var pageNumber = page ?? 1;
			var pagedProducts = await products.ToPagedListAsync(pageNumber, pageSize);

			return PartialView("_ProductPartialView", pagedProducts);
		}
	}
}
