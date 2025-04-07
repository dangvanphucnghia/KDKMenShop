using KDKMenShop.Models;
using KDKMenShop.Models.Model;
using KDKMenShop.Models.ProductModel;
using KDKMenShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace KDKMenShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ThoiTrangNamKDKContext _context;
        public ProductController(ThoiTrangNamKDKContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("id");

            UpdateTTCN();

            //Truyền số lượng mục vào ViewBag để sử dụng trong View

            ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
            ViewBag.ide = HttpContext.Session.GetInt32("id");
            return View();
        }

        public async Task<IActionResult> Detail(int maSP)
        {

            int? userId = HttpContext.Session.GetInt32("id");

            UpdateTTCN();

            //Truyền số lượng mục vào ViewBag để sử dụng trong View

            ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
            ViewBag.ide = HttpContext.Session.GetInt32("id");
            if (maSP == null)
            {
                return RedirectToAction("Index");
            }
            //Dùng AsNoTracking để đọc lại dữ liệu mà kh thay đổi nó 
            var product = await _context.SanPhams
                .AsNoTracking()
                .Include(p => p.Ma.MaVoucherNavigation)
                .FirstOrDefaultAsync(p => p.MaSanPham == maSP);
            var images = await _context.HinhAnhSps
                .AsNoTracking()
                .Where(h => h.MaSanPham == maSP)
                .ToListAsync();

            // Truy vấn dữ liệu từ bảng ChiTietKichThuoc

            var chiTietKichThuocs = await _context.ChiTietKichThuocs
                .Where(ct => ct.MaSanPham == maSP)
                .Select(ct => new ChiTietKichThuoc
                {
                    KichThuoc = ct.KichThuoc,
                    SoLuong = ct.SoLuong
                }).ToListAsync();

            // Tối ưu hóa bằng cách gộp các truy vấn có điều kiện tương tự lại, sử dụng Join để tránh gọi nhiều lần trên cùng một bảng

            var danhGiaSp = await _context.DanhGiaSps
                .AsNoTracking()
                .Where(dg => dg.MaSanPham == maSP)
                .Select(dg => new DanhGiaModel
                {
                    DanhGia = dg.DanhGia,
                    BinhLuan = dg.BinhLuan,
                    Id = dg.Id,
                    KichThuoc = _context.ChiTietDonHangs
                                    .Where(ct => ct.MaDh == dg.MaDh)
                                    .SelectMany(ct => _context.ChiTietKichThuocs.Where(sp => sp.MaSanPham == ct.MaSanPham && sp.MaChiTiet == ct.MaChiTiet).Select(sp => sp.KichThuoc))
                                    .FirstOrDefault(),
                    SoLuong = (int)_context.ChiTietDonHangs.Where(sp => sp.MaDh == dg.MaDh && sp.MaSanPham == dg.MaSanPham).Select(sp => sp.SoLuong).FirstOrDefault(),
                    HinhAnh = dg.HinhAnh,
                    NgayDanhGia = (DateTime)dg.NgayDanhGia,
                    MaSanPham = dg.MaSanPham,
                    TenKhachHang = dg.IdNavigation.IdTkNavigation.HoTen,
                    LuotDanhGia = _context.DanhGiaSps.Count(sp => sp.MaSanPham == dg.MaSanPham),
                    PhanHoi = _context.PhanHoiDanhGia
                        .Where(ph => ph.MaDanhGia == dg.MaDanhGia)
                        .Select(ph => ph.PhanHoi)
                        .FirstOrDefault(),
                    LuotMua = (int)_context.ChiTietDonHangs
                        .Where(sp => sp.MaSanPham == maSP &&
                                     (sp.MaDhNavigation.TrangThaiDh == "Đã đánh giá" ||
                                      (sp.MaDhNavigation.TrangThaiDh == "Đã Thanh Toán" &&
                                       (sp.MaDhNavigation.ThongTinDh == "Đã Giao" || sp.MaDhNavigation.ThongTinDh == "Đang Giao"))))
                        .Sum(sp => sp.SoLuong)
                }).ToListAsync();


            var viewModel = new ProductViewModel
            {
                Product = product,
                Images = images,
                ChiTietKichThuocs = chiTietKichThuocs,
                DanhGiaSps = danhGiaSp,
                FiveStarCount = danhGiaSp.Count(d => d.DanhGia == 5),
                FourStarCount = danhGiaSp.Count(d => d.DanhGia == 4),
                ThreeStarCount = danhGiaSp.Count(d => d.DanhGia == 3),
                TwoStarCount = danhGiaSp.Count(d => d.DanhGia == 2),
                OneStarCount = danhGiaSp.Count(d => d.DanhGia == 1)
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> TimKiemSanPham(string q)
        {

            var danhSachSanPham = await _context.SanPhams
                .AsNoTracking()
				.Where(sp => sp.TenSanPham.StartsWith(q) || sp.TenSanPham.Contains(q))
                .Select(sp => sp.TenSanPham)
                .Distinct()
                .Take(6)
                .ToListAsync();

            return Json(danhSachSanPham);
        }

        public async Task<IActionResult> Search(string searchString)
        {
            IQueryable<SanPham> query = _context.SanPhams.AsNoTracking();

            // Keyword
            var specialSearchTerms = new Dictionary<string, List<int>>
                {
                    { "quần áo", new List<int> { 1, 2, 5 } },
                    { "quan ao", new List<int> { 1, 2, 5 } },
                    { "ao quan", new List<int> { 1, 2, 5 } },
                    { "áo quần", new List<int> { 1, 2, 5 } },
                    { "ba lô", new List<int> { 4 } },
                    { "ba lo", new List<int> { 4 } }
                };

            List<SanPham> returnedProducts;
            if (string.IsNullOrWhiteSpace(searchString))
            {
                returnedProducts = _context.SanPhams
                    .AsNoTracking()
                    .Include(s => s.Ma.MaVoucherNavigation)
                    .ToList();
            }
            else if (searchString.Length == 1)
            {
                returnedProducts = new List<SanPham>();
            }
            else
            {
                if (specialSearchTerms.ContainsKey(searchString.Trim().ToLower()))
                {
                    var loaiSPIds = specialSearchTerms[searchString.Trim().ToLower()];
                    returnedProducts = await query
                        .Where(p => loaiSPIds.Contains(p.MaLoaiSp))
                        .Include(s => s.Ma.MaVoucherNavigation)
                        .ToListAsync();
                }
                else
                {
                    returnedProducts = await query
                        .Where(p => p.TenSanPham.ToLower().Contains(searchString.ToLower()))
                        .Include(s => s.Ma.MaVoucherNavigation)
                        .ToListAsync();
                }
            }
            // Tính toán Precision và Recall
            double precision = 0;
            double recall = 0;
            // Số lượng sản phẩm có thể khớp với từ khóa
            var relevantCount = returnedProducts.Count;

            if (returnedProducts.Count > 0)
            {
                precision = (double)returnedProducts.Count / returnedProducts.Count * 100;
            }

            if (relevantCount > 0)
            {
                recall = (double)returnedProducts.Count / relevantCount * 100;
            }

            ViewBag.Precision = precision;
            ViewBag.Recall = recall;
            int? userId = HttpContext.Session.GetInt32("id");
            UpdateTTCN();
            return View(returnedProducts);
        }

        public void UpdateTTCN()
        {
            int? userId = HttpContext.Session.GetInt32("id");
            var gioHang = _context.GioHangs.AsNoTracking().FirstOrDefault(g => g.Id == userId);
            var wishlist = _context.Wishlists.AsNoTracking().FirstOrDefault(w => w.Id == userId);


            if (gioHang != null)
            {
                // Lấy số lượng các mục trong giỏ hàng từ cơ sở dữ liệu
                string cartId = gioHang.MaGioHang;
                var cartItemCount = _context.ChiTietGioHangs
                    .Where(g => g.MaGioHang == cartId)
                    .Sum(g => g.SoLuong);


                ViewBag.CartItemCount = cartItemCount;
                HttpContext.Session.SetInt32("CartItemCount", cartItemCount);
                var cartItemSum = _context.ChiTietGioHangs
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
                string wishlistID = wishlist.WishlistId;
                var wishlistItemCount = _context.ChiTietWishlists
                    .Where(w => w.WishlistId == wishlistID).Count();

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