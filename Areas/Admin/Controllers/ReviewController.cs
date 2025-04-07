using KDKMenShop.Areas.Admin.Models;
using KDKMenShop.Models;
using KDKMenShop.Models.Authentication;
using KDKMenShop.Models.DAO;
using KDKMenShop.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KDKMenShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReviewController : Controller
    {
        private readonly ThoiTrangNamKDKContext _context;
        public ReviewController(ThoiTrangNamKDKContext context)
        {
            _context = context;
        }
        [AdminAuthorize(MaChucNang = 17)]
        public async Task<IActionResult> DanhSachDanhGia()
        {
            @ViewBag.admin = HttpContext.Session.GetString("User");

            return View(await _context.DanhGiaSps.Include(s => s.IdNavigation).Include(s => s.IdNavigation.IdTkNavigation).OrderByDescending(p => p.MaDanhGia).ToListAsync());
        }
        [AdminAuthorize(MaChucNang = 18)]
        public IActionResult TraLoiDanhGia(string id)
        {
            // Lấy thông tin User từ Session
            @ViewBag.admin = HttpContext.Session.GetString("User");
            // Tạo đối tượng model
            var phanHoi = new PhanHoiDanhGia { MaDanhGia = id };
            return View(phanHoi);
        }

        [HttpPost]
        //[AdminAuthorize(maChucNang = 8)]
        [AdminAuthorize(MaChucNang = 18)]
        public IActionResult TraLoiDanhGia(PhanHoiDanhGia phDG, string id)
        {

            var dg = _context.DanhGiaSps.First(m => m.MaDanhGia == id);

            if (dg == null)
            {
                ViewData["Error"] = "Don't empty!";
            }

			PhanHoiDanhGia phanHoiDanhGia = new PhanHoiDanhGia
			{
				MaDanhGia = dg.MaDanhGia,
				PhanHoi = phDG.PhanHoi,
			};
			_context.PhanHoiDanhGia.Add(phanHoiDanhGia);
            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();
            return RedirectToAction("DanhSachDanhGia");

        }

    }
}
