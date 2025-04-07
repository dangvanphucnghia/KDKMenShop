using KDKMenShop.Models;
using KDKMenShop.Models.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KDKMenShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {

        private readonly ThoiTrangNamKDKContext _context;

        public CategoryController(ThoiTrangNamKDKContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;

        }
        [AdminAuthorize(MaChucNang = 4)]
        public async Task<IActionResult> Index()
        {
            @ViewBag.admin = HttpContext.Session.GetString("User");
            return View(await _context.LoaiSps.OrderByDescending(p => p.MaLoaiSp).ToListAsync());
        }
        [AdminAuthorize(MaChucNang = 5)]
        public IActionResult Create()
        {
            @ViewBag.admin = HttpContext.Session.GetString("User");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize(MaChucNang = 5)]
        public async Task<IActionResult> Create(LoaiSp lsp)
        {

            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên sản phẩm đã tồn tại trong CSDL chưa
                if (_context.LoaiSps.Any(x => x.TenLoaiSp.ToLower() == lsp.TenLoaiSp.ToLower()))
                {
                    TempData["error"] = "Tên sản phẩm đã tồn tại trong CSDL";
                    return RedirectToAction("Index");
                }
                _context.LoaiSps.Add(lsp);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm loại sản phẩm thành công";
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
        [AdminAuthorize(MaChucNang = 6)]
        public async Task<IActionResult> Edit(int id)
        {
            @ViewBag.admin = HttpContext.Session.GetString("User");
            LoaiSp lsp = await _context.LoaiSps.FindAsync(id);

            return View(lsp);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize(MaChucNang = 6)]
        public async Task<IActionResult> Edit(int id, LoaiSp lsp)
        {


            if (ModelState.IsValid)
            {
                var existingCategory = await _context.LoaiSps.FindAsync(id);

                if (existingCategory != null)
                {
                    // Cập nhật thông tin sản phẩm
                    existingCategory.TenLoaiSp = lsp.TenLoaiSp;
                    _context.LoaiSps.Update(existingCategory);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cập nhật loại sản phẩm thành công";
                    return RedirectToAction("Index");

                }
                else

                {
                    TempData["error"] = "Loại Sản phẩm không tồn tại";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                // Xử lý lỗi model
                TempData["error"] = "Lỗi model";
                // Code xử lý lỗi model
                return View(lsp);
            }
        }
        [AdminAuthorize(MaChucNang = 0)]
        public async Task<IActionResult> Delete(int id)
        {
            @ViewBag.admin = HttpContext.Session.GetString("User");
            var lsp = await _context.LoaiSps.FindAsync(id);


            // Xóa sản phẩm từ cơ sở dữ liệu và lưu thay đổi
            _context.LoaiSps.Remove(lsp);
            await _context.SaveChangesAsync();

            TempData["success"] = "Xóa loại sản phẩm thành công";
            return RedirectToAction("Index");
        }

    }
}
