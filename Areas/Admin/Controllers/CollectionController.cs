using KDKMenShop.Models;
using KDKMenShop.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KDKMenShop.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class CollectionController : Controller
	{
		
		private readonly ThoiTrangNamKDKContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public CollectionController(ThoiTrangNamKDKContext context, IWebHostEnvironment webHostEnvironment)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
		}
        [AdminAuthorize(MaChucNang = 7)]
        public async Task<IActionResult> Index()
		{
			@ViewBag.admin = HttpContext.Session.GetString("User");
			return View(await _context.BoSuuTaps.OrderByDescending(p => p.MaBoSuuTap).ToListAsync());
		}
        [AdminAuthorize(MaChucNang = 8)]
        public IActionResult Create()
		{
			@ViewBag.admin = HttpContext.Session.GetString("User");

			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
        [AdminAuthorize(MaChucNang = 8)]
        public async Task<IActionResult> Create(BoSuuTap bst, IFormFile hinhBoSuuTap)
		{

			if (ModelState.IsValid)
			{
				// Kiểm tra xem tên sản phẩm đã tồn tại trong CSDL chưa
				if (_context.BoSuuTaps.Any(x => x.TenBoSuuTap.ToLower() == bst.TenBoSuuTap.ToLower()))
				{
					TempData["error"] = "Bộ Sưu Tập đã tồn tại trong CSDL";
					return RedirectToAction("Index");
				}
				// Kiểm tra xem tên bộ sưu tập đã tồn tại trong CSDL chưa
				if (_context.BoSuuTaps.Any(x => x.MaBoSuuTap == bst.MaBoSuuTap))
				{
					TempData["error"] = "Mã bọ sưu tập đã tồn tại trong CSDL";
					return RedirectToAction("Index");
				}
				if (bst.ImageUpload != null)
				{
					string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
					string imageName = Guid.NewGuid().ToString() + "_" + bst.ImageUpload.FileName;
					string filePath = Path.Combine(uploadDir, imageName);

					FileStream fs = new FileStream(filePath, FileMode.Create);

					await bst.ImageUpload.CopyToAsync(fs);

					fs.Close();
					bst.HinhBoSuuTap = imageName;
				}
				
				// Lưu thông tin bộ sưu tập vào cơ sở dữ liệu
				_context.BoSuuTaps.Add(bst);
				await _context.SaveChangesAsync();
				TempData["success"] = "Thêm Bộ Sưu Tập thành công";
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
        [AdminAuthorize(MaChucNang = 9)]
        public async Task<IActionResult> Edit(int id)
		{
			@ViewBag.admin = HttpContext.Session.GetString("User");
			BoSuuTap bst = await _context.BoSuuTaps.FindAsync(id);

			return View(bst);

		}
		[HttpPost]
		[ValidateAntiForgeryToken]
        [AdminAuthorize(MaChucNang = 9)]
        public async Task<IActionResult> Edit(int id, BoSuuTap bst)
		{


			if (ModelState.IsValid)
			{
				var existingCategory = await _context.BoSuuTaps.FindAsync(id);

				if (existingCategory != null)
				{
					// Cập nhật thông tin sản phẩm
					existingCategory.TenBoSuuTap = bst.TenBoSuuTap;
					_context.BoSuuTaps.Update(existingCategory);
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
				return View(bst);
			}
		}
        [AdminAuthorize(MaChucNang = 0)]
        public async Task<IActionResult> Delete(int id)
		{
			@ViewBag.admin = HttpContext.Session.GetString("User");
			var bst = await _context.BoSuuTaps.FindAsync(id);


			// Xóa sản phẩm từ cơ sở dữ liệu và lưu thay đổi
			_context.BoSuuTaps.Remove(bst);
			await _context.SaveChangesAsync();

			TempData["success"] = "Xóa Bộ Sưu Tập thành công";
			return RedirectToAction("Index");
		}
	}
}
