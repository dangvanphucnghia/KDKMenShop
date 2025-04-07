using KDKMenShop.Models;
using KDKMenShop.Models.Authentication;
using KDKMenShop.Models.ProductModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using KDKMenShop.Models.DAO;
using KDKMenShop.Models.ViewModels;
using KDKMenShop.Models.Model;
using KDKMenShop.Areas.Admin.Models;

namespace KDKMenShop.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class ProductController : Controller
    {

        private readonly ThoiTrangNamKDKContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ThoiTrangNamKDKContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        [AdminAuthorize(MaChucNang = 1)]
        public async Task<IActionResult> Index()
        {


            @ViewBag.admin = HttpContext.Session.GetString("User");
            return View(await _context.SanPhams.OrderByDescending(p => p.MaSanPham).ToListAsync());
        }
        [HttpGet]
        [AdminAuthorize(MaChucNang = 2)]
        public IActionResult Create()
        {

            @ViewBag.admin = HttpContext.Session.GetString("User");
			
			ViewBag.LoaiSps = new SelectList(_context.LoaiSps, "MaLoaiSp", "TenLoaiSp");
            ViewBag.BoSuuTaps = new SelectList(_context.BoSuuTaps, "MaBoSuuTap", "TenBoSuuTap");
			var distinctPrograms = _context.ChiTietKhuyenMais
	.Where(s => s.MaVoucherNavigation.ThoiGianBatDau <= DateTime.Now && s.MaVoucherNavigation.ThoiGianKetThuc >= DateTime.Now)
	.Select(s => s.MaVoucherNavigation.TenChuongTrinh)
	.Distinct()
	.ToList();
            
			ViewBag.MaChiTietGiams = new SelectList(distinctPrograms);
			return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize(MaChucNang = 2)]
        public async Task<IActionResult> Create(SanPham sp)
        {


			if (ModelState.IsValid)
            {
                if (sp.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string imageName = Guid.NewGuid().ToString() + "_" + sp.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);

                    await sp.ImageUpload.CopyToAsync(fs);

                    fs.Close();
                    sp.HinhAnh = imageName;
                }
                _context.SanPhams.Add(sp);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang bị lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }


        }
        [AdminAuthorize(MaChucNang = 3)]
        public async Task<IActionResult> Edit(int id)
        {
            @ViewBag.admin = HttpContext.Session.GetString("User");
            SanPham sp = await _context.SanPhams.FindAsync(id);
            ViewBag.LoaiSps = new SelectList(_context.LoaiSps, "MaLoaiSp", "TenLoaiSp");
			ViewBag.BoSuuTaps = new SelectList(_context.BoSuuTaps, "MaBoSuuTap", "TenBoSuuTap");
			ViewBag.MaChiTietGiams = new SelectList(_context.ChiTietKhuyenMais, "MaChiTietGiam", "Slug", sp.MaChiTietGiam);
			var distinctPrograms = _context.ChiTietKhuyenMais
.Where(s => s.MaVoucherNavigation.ThoiGianBatDau <= DateTime.Now && s.MaVoucherNavigation.ThoiGianKetThuc >= DateTime.Now)
.Select(s => s.MaVoucherNavigation.TenChuongTrinh)
.Distinct()
.ToList();

			ViewBag.MaChiTietGiams = new SelectList(distinctPrograms);
			return View(sp);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize(MaChucNang = 3)]
        public async Task<IActionResult> Edit(int id, SanPham sp)
        {

   //         ViewBag.LoaiSps = new SelectList(_context.LoaiSps, "MaLoaiSp", "TenLoaiSp", sp.MaLoaiSp);
			//ViewBag.MaChiTietGiams = new SelectList(_context.ChiTietKhuyenMais, "MaChiTietGiam", "Slug", sp.MaChiTietGiam);
			if (ModelState.IsValid)
            {
                var existingProduct = await _context.SanPhams.FindAsync(id);

                if (existingProduct != null)
                {
                    // Cập nhật thông tin sản phẩm
                    existingProduct.TenSanPham = sp.TenSanPham;
                    existingProduct.Gia = sp.Gia;
                    existingProduct.MaVatLieu = sp.MaVatLieu;
                    existingProduct.MoTa = sp.MoTa;
                    existingProduct.MaLoaiSp = sp.MaLoaiSp;



                    if (sp.ImageUpload != null)
                    {
                        // Xử lý và cập nhật ảnh mới
                        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        string imageName = Guid.NewGuid().ToString() + "_" + sp.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadDir, imageName);

                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            await sp.ImageUpload.CopyToAsync(fs);
                        }

                        existingProduct.HinhAnh = imageName;
                    }

                    _context.SanPhams.Update(existingProduct);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cập nhật sản phẩm thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Sản phẩm không tồn tại";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                // Xử lý lỗi model
                TempData["error"] = "Lỗi model";
                // Code xử lý lỗi model
                return View(sp);
            }
        }
        [AdminAuthorize(MaChucNang = 0)]
        public async Task<IActionResult> Delete(int id)
        {
            @ViewBag.admin = HttpContext.Session.GetString("User");
            try
            {
                var sp = await _context.SanPhams.FindAsync(id);
                // Xóa hình ảnh liên kết với sản phẩm (nếu có)
                if (!string.IsNullOrEmpty(sp.HinhAnh))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", sp.HinhAnh);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Xóa sản phẩm từ cơ sở dữ liệu và lưu thay đổi
                _context.SanPhams.Remove(sp);
                await _context.SaveChangesAsync();

                TempData["success"] = "Xóa sản phẩm thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                return RedirectToAction("LoiRangBuoc", "RoleError");
            }
            
        }




        //Đơn Hàng
        //public async Task<IActionResult> Delete(int id)
        //{
        //    @ViewBag.admin = HttpContext.Session.GetString("User");
        //    var sp = await _context.SanPhams.FindAsync(id);
        //    // Xóa hình ảnh liên kết với sản phẩm (nếu có)
        //    if (!string.IsNullOrEmpty(sp.HinhAnh))
        //    {
        //        string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", sp.HinhAnh);
        //        if (System.IO.File.Exists(imagePath))
        //        {
        //            System.IO.File.Delete(imagePath);
        //        }
        //    }

        //    // Xóa sản phẩm từ cơ sở dữ liệu và lưu thay đổi
        //    _context.SanPhams.Remove(sp);
        //    await _context.SaveChangesAsync();

        //    TempData["success"] = "Xóa sản phẩm thành công";
        //    return RedirectToAction("Index");
        //}
        // don hang
        //public ActionResult ThemMoi()
        //{

        //    return View();
        //}







        //-----------------------------------------------------------------------@@@@ Đơn Hàng @@@@@ ----------------------------------------------------------------------------------------
        [AdminAuthorize(MaChucNang = 14)]
        public IActionResult DanhSachDonHang()
        {
            @ViewBag.admin = HttpContext.Session.GetString("User");
            var dsdonhang = _context.DonHangs
               .ToList();
			var chiTietDonHang = _context.ChiTietDonHangs.ToList();
            var donHangs = dsdonhang.Select(dh => new OrdersViewModel
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
                DiaChiGiaoHang = dh.DiaChiGiaoHang
            }).ToList();
            return View(donHangs);
        }

        [AdminAuthorize(MaChucNang = 15)]
        public IActionResult ThemMoiDonHang()
        {
            // Lấy thông tin User từ Session
            @ViewBag.admin = HttpContext.Session.GetString("User");
            // Tạo đối tượng model
            var dh = new OrdersViewModel();
            // Truy vấn dữ liệu từ cả hai bảng
            dh.DanhSachSanPham = _context.SanPhams
                .Include(s => s.MaLoaiSpNavigation)
                .SelectMany(s => s.ChiTietKichThuocs.Select(ct => new SanPhamViewModel
                {
                    MaSanPham = s.MaSanPham,
                    TenSanPham = s.TenSanPham,
                    HinhAnh = s.HinhAnh,
                    MaChiTiet = ct.MaChiTiet,
                    KichThuoc = ct.KichThuoc,
                    SoLuong = ct.SoLuong
                }))
                .ToList();
            return View(dh);
        }

        [HttpPost]
        [AdminAuthorize(MaChucNang = 15)]
        public IActionResult ThemMoiDonHang(IFormCollection collection, OrdersViewModel dh)
        {
            if (ModelState.IsValid)
            {
                // Tạo một đối tượng ThongTinCaNhan mới và lưu thông tin cá nhân
                var donHang = new DonHang
                {
                    MaDh = dh.MaDH,
                    NgayLap = dh.NgayLap,
                    TongTien = dh.TongTien,
                    DiaChiGiaoHang = dh.DiaChiGiaoHang,
                    TrangThaiDh = dh.TrangThaiDH,
                    ThongTinDh = dh.ThongTinDH
                };

                _context.DonHangs.Add(donHang);
                _context.SaveChanges();

                string newMaDH = donHang.MaDh;
                foreach (var sanpham in dh.DanhSachSanPham)
                {
                    if (sanpham.Selected)
                    {
                        

                        var newCTDH = new ChiTietDonHang
                        {
                            MaDh = newMaDH,
                            MaGioHang = "64779", // Mã hóa password và lưu vào cơ sở dữ liệu
                            MaSanPham = sanpham.MaSanPham,
                            MaChiTiet =sanpham.MaChiTiet,
                            SoLuong = int.Parse(dh.SoLuong)
                        };
                        // Lấy thông tin chi tiết sản phẩm
                        var chiTietSanPham = _context.ChiTietKichThuocs.FirstOrDefault(ct => ct.MaSanPham == sanpham.MaSanPham && ct.MaChiTiet == sanpham.MaChiTiet);
                        if (chiTietSanPham != null)
                        {
                            
                            chiTietSanPham.SoLuong -= int.Parse(dh.SoLuong);
                           
                        }
                        _context.ChiTietDonHangs.Add(newCTDH);
                    }
                   
                    
                }
                _context.SaveChanges();

                return RedirectToAction("DanhSachDonHang");

            }

            return View(dh);
        }

        [AdminAuthorize(MaChucNang = 0)]
        public async Task<IActionResult> XoaDonhang(string id)
        {
            @ViewBag.admin = HttpContext.Session.GetString("User");
            var dh = await _context.DonHangs.FindAsync(id);
            var ctdhs = _context.ChiTietDonHangs.Where(ct => ct.MaDh == id).ToList();
            if (dh != null)
            {
                // Remove the order
                _context.DonHangs.Remove(dh);

                // Remove the associated details
                foreach (var ctdh in ctdhs)
                {
                    _context.ChiTietDonHangs.Remove(ctdh);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                TempData["success"] = "Xóa đơn hàng thành công";
            }
            else
            {
                TempData["error"] = "Đơn hàng không tồn tại";
            }

            return RedirectToAction("DanhSachDonHang");
        }
        [AdminAuthorize(MaChucNang = 16)]
        public IActionResult SuaDonHangTheoID(string id)
        {
			@ViewBag.admin = HttpContext.Session.GetString("User");
			var dh = _context.DonHangs.First(m => m.MaDh == id);
            return View(dh);
        }
        [AdminAuthorize(MaChucNang = 16)]
        public IActionResult SuaDonHang(string id, IFormCollection collection)
        {

            var dh = _context.DonHangs.First(m => m.MaDh == id);
            var maDH = collection["MaDh"];


            if (string.IsNullOrEmpty(maDH))
            {
                ViewData["Error"] = "Don't empty!";
            }
            else
            {
                dh.MaDh = maDH.ToString();

                TryUpdateModelAsync(dh);
                _context.SaveChanges();
                var chiTietDonHang = _context.ChiTietDonHangs
                           .Include(g => g.MaGioHangNavigation) // truy vấn MaGioHangNavigation từ ChiTietDonHang ra
                           .FirstOrDefault(g => g.MaDh == dh.MaDh);

                if (chiTietDonHang != null)
                {
                    int userId = chiTietDonHang.MaGioHangNavigation?.Id ?? 0;
                    if (userId != 0)
                    {
                        var user = _context.TaiKhoans
                          .Include(t => t.IdTkNavigation) //truy vấn IdTkNavigation từ bảng ThongTintaiKhoan ra
                          .FirstOrDefault(u => u.Id == userId);
                        string userEmail = "";
                        if (user != null)
                        {
                            userEmail = user.IdTkNavigation.Email;
                            string subject = "Thông báo: Đơn hàng đã được cập nhật";
                            string body = $"Đơn hàng của bạn (Mã đơn hàng: {dh.MaDh}) đã được cập nhật thành công. Thông Tin Đơn Hàng Hiện Tại Của Bạn Là: {dh.ThongTinDh}";

                            DAO.SendtoEmail(userEmail, subject, body);



                        }

                        return RedirectToAction("DanhSachDonHang");
                    }
                }
            }
                  
            return SuaDonHangTheoID(id);
        }





        //size
        [AdminAuthorize(MaChucNang = 11)]
        public async Task<IActionResult> DanhSachKichThuoc()
        {
			@ViewBag.admin = HttpContext.Session.GetString("User");
			return View(await _context.ChiTietKichThuocs.Include(s=>s.MaSanPhamNavigation).OrderBy(p => p.MaSanPham).ToListAsync());
		}
        [AdminAuthorize(MaChucNang = 13)]
        public async Task<IActionResult> SuaKichThuoc(int id)
        {
            @ViewBag.admin = HttpContext.Session.GetString("User");
            ChiTietKichThuoc ktsp = await _context.ChiTietKichThuocs.Include(s => s.MaSanPhamNavigation).SingleOrDefaultAsync(k => k.MaChiTiet == id);

			return View(ktsp);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize(MaChucNang = 13)]
        public async Task<IActionResult> SuaKichThuoc(int id, ChiTietKichThuoc ktsp)
        {

            //         ViewBag.LoaiSps = new SelectList(_context.LoaiSps, "MaLoaiSp", "TenLoaiSp", sp.MaLoaiSp);
            //ViewBag.MaChiTietGiams = new SelectList(_context.ChiTietKhuyenMais, "MaChiTietGiam", "Slug", sp.MaChiTietGiam);
            if (ModelState.IsValid)
            {
                var existingProduct = await _context.ChiTietKichThuocs.SingleOrDefaultAsync(k => k.MaChiTiet == id);

                if (existingProduct != null)
                {
                    // Cập nhật thông tin sản phẩm
                   
                    existingProduct.SoLuong = ktsp.SoLuong;

                    _context.ChiTietKichThuocs.Update(existingProduct);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cập nhật chi tiết kích thước sản phẩm thành công";
                    return RedirectToAction("DanhSachKichThuoc");
                }
                else
                {
                    TempData["error"] = "Kích thước sản phẩm không tồn tại";
                    return RedirectToAction("DanhSachKichThuoc");
                }
            }
            else
            {
                // Xử lý lỗi model
                TempData["error"] = "Lỗi model";
                // Code xử lý lỗi model
                return View(ktsp);
            }
        }
        [AdminAuthorize(MaChucNang = 12)]
        public IActionResult ThemMoiKichThuoc()
        {

            @ViewBag.admin = HttpContext.Session.GetString("User");

            // Lấy danh sách mã sản phẩm từ bảng SanPham
            var maSanPhamList = _context.SanPhams.Select(sp => sp.MaSanPham).ToList();

            // Truyền danh sách mã sản phẩm tới View sử dụng ViewBag hoặc ViewModel
            ViewBag.MaSanPhamList = maSanPhamList;
			// Lấy thông tin hình ảnh và các kích thước của sản phẩm đầu tiên trong danh sách (có thể thay đổi tùy vào yêu cầu)
			

			return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize(MaChucNang = 12)]
        public async Task<IActionResult> ThemMoiKichThuoc(ChiTietKichThuoc kt)
        {


            if (ModelState.IsValid)
            {
				// Kiểm tra xem sản phẩm có mã kt.MaSanPham đã có kích thước chưa
				var existingProduct = _context.ChiTietKichThuocs
					.FirstOrDefault(k => k.MaSanPham == kt.MaSanPham && k.KichThuoc == kt.KichThuoc);

				if (existingProduct != null)
				{
					// Nếu đã có kích thước cho sản phẩm này, thông báo lỗi
					TempData["error"] = "Sản phẩm đã có kích thước này.";
					return RedirectToAction("DanhSachKichThuoc");
				}

				_context.ChiTietKichThuocs.Add(kt);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm kích thước sản phẩm thành công";
                return RedirectToAction("DanhSachKichThuoc");
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang bị lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }


        }
        [AdminAuthorize(MaChucNang = 0)]
        public async Task<IActionResult> XoaKichThuoc(int id)
        {

            try
            {
                @ViewBag.admin = HttpContext.Session.GetString("User");
                var kt = await _context.ChiTietKichThuocs.FirstOrDefaultAsync(s => s.MaChiTiet == id);



                // Xóa sản phẩm từ cơ sở dữ liệu và lưu thay đổi
                _context.ChiTietKichThuocs.Remove(kt);
                await _context.SaveChangesAsync();

                TempData["success"] = "Xóa kích thước thành công";
                return RedirectToAction("DanhSachKichThuoc");
            }
            catch (Exception)
            {

                return RedirectToAction("LoiRangBuoc", "RoleError");
            }
        }



    }
}
