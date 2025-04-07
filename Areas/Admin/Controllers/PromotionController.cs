using KDKMenShop.Areas.Admin.Models;
using KDKMenShop.Models;
using KDKMenShop.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KDKMenShop.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class PromotionController : Controller
	{
		private readonly ThoiTrangNamKDKContext _context;
		public PromotionController(ThoiTrangNamKDKContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			return View();
		}
        //khuyến mãi cho sản phẩm
        [AdminAuthorize(MaChucNang = 10)]
        public IActionResult KhuyenMai()
		{

			@ViewBag.admin = HttpContext.Session.GetString("User");
			var model = new KhuyenMaiViewModel();

			// Lấy danh sách sản phẩm từ database và đưa vào model
			model.DanhSachSanPham = _context.SanPhams.Include(s => s.MaLoaiSpNavigation).Select(s => new SanPhamViewModel
			{
				MaSanPham = s.MaSanPham,
				TenSanPham = s.TenSanPham,
				MaChiTietGiam = s.MaLoaiSp,
				TenGiamGia = "Giảm " +s.MaLoaiSpNavigation.TenLoaiSp,
				Slug = "giam-" + s.MaLoaiSpNavigation.Slug,
				HinhAnh = s.HinhAnh
			}).ToList();

			return View(model);
		}

		[HttpPost]
        [AdminAuthorize(MaChucNang = 10)]
        public IActionResult KhuyenMai(KhuyenMaiViewModel model, string searchText)
		{
			if (ModelState.IsValid)
			{
				// Xử lý lưu thông tin khuyến mãi vào database
				var khuyenMai = new KhuyenMai
				{
					MaVoucher = GenerateRandomVoucherCode(),
					TenChuongTrinh = model.TenChuongTrinh,
					ThoiGianBatDau = model.ThoiGianBatDau,
					ThoiGianKetThuc = model.ThoiGianKetThuc,
					GhiChu = model.GhiChu,
					PhanTramGiam = model.PhanTramGiam
				};

				_context.KhuyenMais.Add(khuyenMai);
				_context.SaveChanges();

				string maKhuyenMai = khuyenMai.MaVoucher;

				if (model.ChonTatCaSanPham)
				{
					foreach (var sanpham in model.DanhSachSanPham)
					{
						//var existingChiTietKhuyenMai = _context.ChiTietKhuyenMais.FirstOrDefault(ct => ct.MaSanPham == sanpham.MaSanPham && ct.MaChiTietGiam == sanpham.MaChiTietGiam.Value);
						//if (existingChiTietKhuyenMai != null)
						//{
						//	// Nếu đã tồn tại, cập nhật lại MaVoucher của bản ghi đó
						//	existingChiTietKhuyenMai.MaVoucher = maKhuyenMai;
						//	_context.ChiTietKhuyenMais.Update(existingChiTietKhuyenMai);
						//}
						//else
						//{
							_context.ChiTietKhuyenMais.Add(new ChiTietKhuyenMai
							{
								MaSanPham = sanpham.MaSanPham,
								MaChiTietGiam = (int)sanpham.MaChiTietGiam.Value,
								TenGiamGia = sanpham.TenGiamGia,
								Slug = sanpham.Slug,
								MaVoucher = maKhuyenMai
							});
							// Cập nhật lại MaChiTietGiam của sản phẩm
							var existingSanPham = _context.SanPhams.FirstOrDefault(s => s.MaSanPham == sanpham.MaSanPham);
							if (existingSanPham != null)
							{
								existingSanPham.MaChiTietGiam = sanpham.MaChiTietGiam.Value;
								_context.SanPhams.Update(existingSanPham);
							}
						//}
					}



				}
				else
				{
					foreach (var sanpham in model.DanhSachSanPham)
					{
						//var existingChiTietKhuyenMai = _context.ChiTietKhuyenMais.FirstOrDefault(ct => ct.MaSanPham == sanpham.MaSanPham && ct.MaChiTietGiam == sanpham.MaChiTietGiam.Value);
						//if (existingChiTietKhuyenMai != null)
						//{
						//	// Nếu đã tồn tại, cập nhật lại MaVoucher của bản ghi đó
						//	existingChiTietKhuyenMai.MaVoucher = maKhuyenMai;
						//	_context.ChiTietKhuyenMais.Update(existingChiTietKhuyenMai);
						//}
						//else
						//{
							if (sanpham.Selected)
							{
								_context.ChiTietKhuyenMais.Add(new ChiTietKhuyenMai
								{
									MaSanPham = sanpham.MaSanPham,
									MaChiTietGiam = sanpham.MaChiTietGiam.Value,
									Slug = sanpham.Slug,
									TenGiamGia = sanpham.TenGiamGia,
									MaVoucher = maKhuyenMai
								});

								// Cập nhật lại MaChiTietGiam của sản phẩm
								var existingSanPham = _context.SanPhams.FirstOrDefault(s => s.MaSanPham == sanpham.MaSanPham);
								if (existingSanPham != null)
								{
									existingSanPham.MaChiTietGiam = sanpham.MaChiTietGiam.Value;
									
								_context.SanPhams.Update(existingSanPham);
								}
								var existingChiTietKhuyenMai = _context.ChiTietKhuyenMais.FirstOrDefault(ct => ct.MaSanPham == sanpham.MaSanPham && ct.MaChiTietGiam == sanpham.MaChiTietGiam.Value);
								if (existingChiTietKhuyenMai != null)
								{
									// Nếu đã tồn tại, cập nhật lại MaVoucher của bản ghi đó
									existingChiTietKhuyenMai.MaVoucher = maKhuyenMai;
									_context.ChiTietKhuyenMais.Update(existingChiTietKhuyenMai);
								}

						}
						//}
					}
				}

				_context.SaveChanges();

				return RedirectToAction("Dasboard", "Home");
			}
			// Nếu ModelState không hợp lệ, quay lại view với model và searchText
			model.DanhSachSanPham = _context.SanPhams.Include(s => s.MaLoaiSpNavigation)
										.Where(s => s.TenSanPham.ToLower().Contains(searchText.ToLower()))
										.Select(s => new SanPhamViewModel
										{
											MaSanPham = s.MaSanPham,
											TenSanPham = s.TenSanPham,
											MaChiTietGiam = s.MaLoaiSp,
											TenGiamGia = "Giảm " + s.MaLoaiSpNavigation.TenLoaiSp,
											Slug = "giam-" + s.MaLoaiSpNavigation.Slug
										})
										.ToList();
			return View(model);
		}
		private string GenerateRandomVoucherCode()
		{
			var random = new Random();
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, 10)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}
	}
}
