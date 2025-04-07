using KDKMenShop.Models;
using KDKMenShop.Models.ProductModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace KDKMenShop.Controllers
{
    public class PromotionController : Controller
    {
        private readonly ThoiTrangNamKDKContext _context;
        public PromotionController(ThoiTrangNamKDKContext context)
        {
            _context = context;
        }
        public void UpdateTTCN()
        {
            int? userId = HttpContext.Session.GetInt32("id");
            if (userId.HasValue)
            {
                // Lấy thông tin giỏ hàng
                var gioHang = _context.GioHangs
                    .AsNoTracking()
                    .FirstOrDefault(g => g.Id == userId);

                if (gioHang != null)
                {
                    string cartId = gioHang.MaGioHang;

                    var cartSummary = _context.ChiTietGioHangs
                        .Where(g => g.MaGioHang == cartId)
                        .GroupBy(g => g.MaGioHang)
                        .Select(g => new
                        {
                            TotalItems = g.Sum(item => item.SoLuong),
                            TotalPrice = g.Sum(item => item.SoLuong * item.Gia)
                        })
                        .FirstOrDefault();

                    ViewBag.CartItemCount = cartSummary?.TotalItems ?? 0;
                    HttpContext.Session.SetInt32("CartItemCount", cartSummary?.TotalItems ?? 0);
                    ViewBag.CartItemSum = cartSummary?.TotalPrice ?? 0;
                    HttpContext.Session.SetInt32("CartItemSum", (int)(cartSummary?.TotalPrice ?? 0));
                }

                // Lấy thông tin wishlist
                var wishlist = _context.Wishlists
                    .AsNoTracking()
                    .FirstOrDefault(w => w.Id == userId);

                if (wishlist != null)
                {
                    string wishlistID = wishlist.WishlistId;
                    int wishlistItemCount = _context.ChiTietWishlists
                        .Count(w => w.WishlistId == wishlistID);

                    ViewBag.WishlistItemCount = wishlistItemCount;
                    HttpContext.Session.SetInt32("WishlistItemCount", wishlistItemCount);
                }
            }
            else
            {
                ViewBag.CartItemCount = ViewBag.CartItemSum = ViewBag.WishlistItemCount = 0;
                HttpContext.Session.SetInt32("CartItemCount", 0);
                HttpContext.Session.SetInt32("CartItemSum", 0);
                HttpContext.Session.SetInt32("WishlistItemCount", 0);
            }

            ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
            ViewBag.ide = HttpContext.Session.GetInt32("id");
        }
        private bool KiemTraKhuyenMai(KhuyenMai khuyenmai)
        {
            DateTime dateTime = DateTime.Now;
            return khuyenmai.ThoiGianBatDau <= dateTime && dateTime <= khuyenmai.ThoiGianKetThuc;
        }
        public async Task<IActionResult> Index(int? page, string Slug = "", string category = "", decimal? minPrice = null, decimal? maxPrice = null)
        {
            const int pageSize = 4; // Number of items per page
            // Lấy số lượng sản phẩm từ Session
            UpdateTTCN();
            var ctkm = await _context.ChiTietKhuyenMais
                                    .Include(s => s.MaVoucherNavigation)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(s => s.Slug == Slug);

            if (ctkm == null)
            {
                return RedirectToAction("Index");
            }
            var categories = await _context.LoaiSps
                   .Select(c => c.TenLoaiSp)
                   .ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.Slug = Slug;

            var products = _context.SanPhams.AsNoTracking()
                          .Where(s => s.Ma.Slug == ctkm.Slug && s.Ma.MaVoucherNavigation.ThoiGianBatDau <= DateTime.Now &&
                                s.Ma.MaVoucherNavigation.ThoiGianKetThuc >= DateTime.Now).Include(s => s.Ma.MaVoucherNavigation)
                          .OrderByDescending(s => s.MaSanPham).Distinct().AsQueryable(); ; // Add Distinct to avoid duplication;
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.MaLoaiSpNavigation.TenLoaiSp == category);
            }
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Gia >= minPrice);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Gia <= maxPrice);
            }
            var pagedProducts = await products.ToPagedListAsync(page ?? 1, pageSize);
            return View(pagedProducts);
        }
        public IActionResult Home()
        {
            int? userId = HttpContext.Session.GetInt32("id");
            UpdateTTCN();
            var sanpham = _context.SanPhams.AsNoTracking().Include(s => s.Ma.MaVoucherNavigation).Include(s => s.MaLoaiSpNavigation).ToList();
            return View(sanpham);
        }
        [HttpGet]
        public IActionResult TimKiemSanPhamGiamGia(string q)
        {

            var danhSachSanPhamGiamGia = _context.SanPhams.AsNoTracking().Include(s => s.Ma.MaVoucherNavigation).Include(s => s.Ma)
                                .Where(sp => sp.MaChiTietGiam != null && sp.Ma.MaVoucherNavigation.ThoiGianBatDau <= DateTime.Now
                                    && sp.Ma.MaVoucherNavigation.ThoiGianKetThuc >= DateTime.Now && (sp.TenSanPham.StartsWith(q) || sp.TenSanPham.Contains(q)))
                                .Select(sp => sp.TenSanPham)
                                .Distinct()
                                .Take(6)
                                .ToList();

            return Json(danhSachSanPhamGiamGia);
        }
        public IActionResult SearchPromotion(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                searchString = "";
            }
            else
            {
                searchString = searchString.Trim();
            }

            var products = _context.SanPhams.AsNoTracking().Include(s => s.Ma.MaVoucherNavigation).Include(s => s.Ma)
                            .Where(p => p.MaChiTietGiam != null && p.Ma.MaVoucherNavigation.ThoiGianBatDau <= DateTime.Now
                                   && p.Ma.MaVoucherNavigation.ThoiGianKetThuc >= DateTime.Now && p.TenSanPham.Contains(searchString))
                            .ToList();
            int? userId = HttpContext.Session.GetInt32("id");
            UpdateTTCN();
            return View(products);
        }
        public async Task<IActionResult> FilterProducts(string category, decimal? minPrice, decimal? maxPrice, int? page, string slug)
        {
            const int pageSize = 4;

            var ctkm = await _context.ChiTietKhuyenMais
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Slug == slug);

            if (ctkm == null)
            {
                return PartialView("_ProductPartialView", new List<SanPham>());
            }

            var products = _context.SanPhams
                .AsNoTracking()
                .Where(p => p.MaChiTietGiam == ctkm.MaChiTietGiam);

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.MaLoaiSpNavigation.TenLoaiSp == category);
            }
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Gia >= minPrice);
                if (maxPrice.HasValue)
                {
                    products = products.Where(p => p.Gia <= maxPrice);
                }
            }
            var pagedProducts = await products.ToPagedListAsync(page ?? 1, pageSize);
            return PartialView("_ProductPartialView", pagedProducts);
        }
    }
}
