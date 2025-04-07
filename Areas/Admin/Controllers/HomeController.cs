using KDKMenShop.Areas.Admin.Models;
using KDKMenShop.Models;
using KDKMenShop.Models.Model;
using KDKMenShop.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KDKMenShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ThoiTrangNamKDKContext _context;
        public HomeController(ThoiTrangNamKDKContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dasboard()
        {
            @ViewBag.idtk = HttpContext.Session.GetString("UserType");
            @ViewBag.admin = HttpContext.Session.GetString("User");
            if (HttpContext.Session.GetString("User") == null)
            {
                
                return RedirectToAction("DangNhap");
            }
            var donHangs = _context.DonHangs.Select(dh => new OrdersViewModel
            {
                MaDH = dh.MaDh,
                NgayLap = dh.NgayLap,
                TongTien = dh.TongTien,
                TrangThaiDH = dh.TrangThaiDh,
                TenSanPham = string.Join(", ", _context.ChiTietDonHangs
            .Where(ct => ct.MaDh == dh.MaDh)
            .SelectMany(ct => _context.SanPhams.Where(sp => sp.MaSanPham == ct.MaSanPham).Select(sp => sp.TenSanPham))
            .ToList()),
                ThongTinDH = dh.ThongTinDh,
                KichThuoc = string.Join(", ", _context.ChiTietDonHangs
            .Where(ct => ct.MaDh == dh.MaDh)
            .SelectMany(ct => _context.ChiTietKichThuocs.Where(sp => sp.MaSanPham == ct.MaSanPham && sp.MaChiTiet == ct.MaChiTiet).Select(sp => sp.KichThuoc))
            .ToList()),
                SoLuong = string.Join(", ", _context.ChiTietDonHangs
            .Where(ct => ct.MaDh == dh.MaDh)
            .Select(ct => ct.SoLuong)
            .ToList()),
                MaSanPham = string.Join(", ", _context.ChiTietDonHangs
            .Where(ct => ct.MaDh == dh.MaDh)
            .SelectMany(ct => _context.SanPhams.Where(sp => sp.MaSanPham == ct.MaSanPham).Select(sp => sp.MaSanPham))
            .ToList()),
                DiaChiGiaoHang = dh.DiaChiGiaoHang,
                TenKhachHang = _context.ChiTietDonHangs
    .Where(ct => ct.MaDh == dh.MaDh)
    .Select(ct => _context.TaiKhoans
        .Where(tk => tk.Id == ct.MaGioHangNavigation.Id)
        .Select(tk => tk.TenDangNhap)
        .FirstOrDefault())
    .FirstOrDefault()
            }).ToList();
            // Truy xuất dữ liệu doanh thu từ danh sách đơn hàng
            // Nhóm dữ liệu đơn hàng theo tháng và tính tổng doanh thu cho mỗi tháng
            var revenueByMonth = donHangs.Where(dh => dh.TrangThaiDH != "Chưa Thanh Toán" && dh.ThongTinDH == "Đã Giao")
                .GroupBy(dh => new { Month = dh.NgayLap.Month, Year = dh.NgayLap.Year })
                .Select(group => new
                {
                    Thang = group.Key.Month,
                    Nam = group.Key.Year,
                    DoanhThu = group.Sum(dh => dh.TongTien)
                })
                .OrderBy(group => group.Nam)
                .ThenBy(group => group.Thang)
                .ToList();

            // Tạo các mảng chứa nhãn (labels) và dữ liệu (data) cho biểu đồ
            var labels = revenueByMonth.Select(item => $"{item.Thang}/{item.Nam}").ToArray();
            var data = revenueByMonth.Select(item => item.DoanhThu).ToArray();


            // Lấy số lượng sản phẩm
            var productCount = await _context.SanPhams.CountAsync();

            // Lấy số lượng đơn hàng
            var orderCount = await _context.DonHangs.CountAsync();

            // Lấy số lượng tài khoản
            var accountCount = await _context.TaiKhoans.Include(s=>s.IdTkNavigation).Where(s=>s.IdTkNavigation.TrangThai != "Khóa").CountAsync();


            var label1 = new[] { "Sản Phẩm", "Đơn Hàng", "Tài Khoản" };
            var data1 = new[] { productCount, 0 ,0 };
            var data2 = new[] { 0, orderCount, 0 };
            var data3 = new[] { 0, 0, accountCount };
            var danhSachSanPham = await _context.SanPhams.OrderByDescending(p => p.MaSanPham).ToListAsync();
            var viewModel = new DashboardViewModel
            {
                DanhSachDonHang = donHangs,
                DanhSachSanPham = danhSachSanPham,
                //Doanh thu
                Labels = labels,
                Data = data,
                // Sản phẩm , Đơn hàng , Tài Khoản
                Labels1 = label1,
                Data1 = data1,
                Data2 = data2,
                Data3 = data3,
            };
            return View(viewModel);
        }
        public IActionResult DangNhap()
        {
           
            if (HttpContext.Session.GetString("User") != null)
            {
                @ViewBag.admin = HttpContext.Session.GetString("User");
                //ViewBag.ide = HttpContext.Session.GetInt32("id");
                

            }
            return View();
        }

        [HttpPost]
        public IActionResult DangNhap(string taiKhoan, string matKhau)
        {
            if (string.IsNullOrWhiteSpace(taiKhoan) || string.IsNullOrWhiteSpace(matKhau))
            {
                @TempData["error"] = "Tên đăng nhập và mật khẩu không được để trống.";
                return View("DangNhap");
            }
            matKhau = MaHoaPassword.GetMd5Hash(matKhau);
            var uservar = _context.TaiKhoans.Include(s=>s.IdTkNavigation).SingleOrDefault(s => s.TenDangNhap.ToLower() == taiKhoan.ToLower() && s.MatKhau == matKhau);
			var currentTime = DateTime.Now;
			if (uservar != null )
            {
                var thongTinCaNhan = _context.ThongTinCaNhans.First(t => t.IdTk == uservar.IdTk);
                if (uservar.LoaiTk.Trim().ToLower() == "user")
                {
                    @TempData["error"] = "Người dùng không được phép đăng nhập.";
                    return View();
                }
                else if(thongTinCaNhan != null && thongTinCaNhan.TrangThai != "Khóa")
                {
                    HttpContext.Session.SetString("User", uservar.TenDangNhap);
                    var jsonSettings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    HttpContext.Session.SetString("Users", JsonConvert.SerializeObject(uservar, jsonSettings));

                    //HttpContext.Session.SetString("Users", uservar.ToString());
                    HttpContext.Session.SetString("UserType", uservar.LoaiTk.Trim());
                    HttpContext.Session.SetString("UserId", uservar.Id.ToString());

                    return RedirectToAction("Dasboard", "Home");
                }
                else if (currentTime > thongTinCaNhan.NgayKetThuc)
                {
                    HttpContext.Session.SetString("User", uservar.TenDangNhap);
                    var jsonSettings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                    thongTinCaNhan.TrangThai = "Mở";

                    _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                    HttpContext.Session.SetString("Users", JsonConvert.SerializeObject(uservar, jsonSettings));

                    //HttpContext.Session.SetString("Users", uservar.ToString());
                    HttpContext.Session.SetString("UserType", uservar.LoaiTk.Trim());
                    HttpContext.Session.SetString("UserId", uservar.Id.ToString());
                    return RedirectToAction("Dasboard", "Home");
                }
                // Đăng nhập thất bại vì tài khoản đang bị khóa
                var remainingTime = thongTinCaNhan.NgayKetThuc - DateTime.Now;
                ViewBag.RemainingTime = remainingTime;
                @TempData["error"] = "Tài khoản đã bị khóa từ " + thongTinCaNhan.NgayBatDau?.ToString("dd/MM/yyyy") + " đến " + thongTinCaNhan.NgayKetThuc?.ToString("dd/MM/yyyy");
				return View("DangNhap");

            }
            else
            {
                TempData["error"] = "Tài khoản không đúng";
                return View("DangNhap");
            }
        }


        //Đăng xuất
        public async Task<IActionResult> DangXuatAsync()
        {
            HttpContext.Session.Remove("User");
            await HttpContext.SignOutAsync();
            return RedirectToAction("DangNhap");

        }
    }
}
