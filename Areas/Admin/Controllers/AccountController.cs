using KDKMenShop.Areas.Admin.Models;
using KDKMenShop.Models;
using KDKMenShop.Models.Authentication;
using KDKMenShop.Models.DAO;
using KDKMenShop.Models.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

namespace KDKMenShop.Areas.Admin.Controllers
{
	[Area("Admin")]
    public class AccountController : Controller
    {
		private readonly ThoiTrangNamKDKContext _context;
		public AccountController(ThoiTrangNamKDKContext context)
		{
			_context = context;
		}
   
        public IActionResult Index()
        {
            return View();
        }

        [AdminAuthorize(MaChucNang = 19)]
        public ActionResult DanhSachTaiKhoan()
		{
            @ViewBag.idtk = HttpContext.Session.GetString("UserType");
            @ViewBag.admin = HttpContext.Session.GetString("User");
            var dstaiKhoan = _context.TaiKhoans.Include(s=>s.IdTkNavigation).ToList();



			return View(dstaiKhoan);
		}
        [AdminAuthorize(MaChucNang = 20)]
        public IActionResult ThemMoiUser()
		{
			@ViewBag.idtk = HttpContext.Session.GetString("UserType");
			@ViewBag.admin = HttpContext.Session.GetString("User");

			return View();
        }

        [HttpPost]
        [AdminAuthorize(MaChucNang = 20)]
        public IActionResult ThemMoiUser(AccountModel user)
        {
            if (ModelState.IsValid)
            {
                // Tạo một đối tượng ThongTinCaNhan mới và lưu thông tin cá nhân
                var newThongTinCaNhan = new ThongTinCaNhan
                {
                    HoTen = user.Name,
                    Email = user.Email,
                    Sdt = user.SDT,
                    DiaChi = user.Diachi,
                    IsEmailConfirmed = true
                };

                _context.ThongTinCaNhans.Add(newThongTinCaNhan);
                _context.SaveChanges();

                int newIdTK = newThongTinCaNhan.IdTk;

                var newUser = new TaiKhoan
                {
                    TenDangNhap = user.TaiKhoan,
                    MatKhau = MaHoaPassword.GetMd5Hash(user.Password), // Mã hóa password và lưu vào cơ sở dữ liệu
					LoaiTk = user.loaiTK,
                    IdTk = newIdTK
                };

                _context.TaiKhoans.Add(newUser);
                _context.SaveChanges();

                return RedirectToAction("DanhSachTaiKhoan");
            }

            return View(user);
        }
        [AdminAuthorize(MaChucNang = 21)]
        public IActionResult SuaTaiKhoan(int id)
		{
			var taikhoan = _context.TaiKhoans.Include(s => s.IdTkNavigation).First(m => m.Id == id);
			return View(taikhoan);
		}

		[HttpPost]
        [AdminAuthorize(MaChucNang = 21)]
        public IActionResult SuaTaiKhoan(int id, IFormCollection collection)
		{
			var taikhoan = _context.TaiKhoans.Include(s => s.IdTkNavigation).First(m => m.Id == id);
			var DangNhap = collection["TenDangNhap"];
			var MatKhau = collection["MatKhau"];


			if (string.IsNullOrEmpty(DangNhap))
			{
				ViewData["Error"] = "Don't empty!";
			}
			else
			{
				taikhoan.MatKhau = MatKhau;
				TryUpdateModelAsync(taikhoan);
				_context.SaveChanges();
				return RedirectToAction("DanhSachTaiKhoan");
			}
			return this.SuaTaiKhoan(id);
		}
        [AdminAuthorize(MaChucNang = 0)]
        public IActionResult XoaTaiKhoan(int id)
		{

			var taikhoan = _context.TaiKhoans.First(m => m.Id == id);
			return View(taikhoan);

		}

		[HttpPost]
        [AdminAuthorize(MaChucNang = 0)]
        public IActionResult XoaTaiKhoan(int id, IFormCollection collection)
		{
			var tk = _context.TaiKhoans.Where(m => m.Id == id).First();
			_context.TaiKhoans.Remove(tk);
			_context.SaveChanges();
			return RedirectToAction("DanhSachTaiKhoan");
		}
        [AdminAuthorize(MaChucNang = 22)]
        public IActionResult TuyChonTaiKhoan(int id)
		{
			var taikhoan = _context.TaiKhoans.Include(s => s.IdTkNavigation).First(m => m.Id == id);
			return View(taikhoan);
		}

		[HttpPost]
        [AdminAuthorize(MaChucNang = 22)]
        public IActionResult TuyChonTaiKhoan(int id, IFormCollection collection)
		{
			var taikhoan = _context.TaiKhoans.Include(s => s.IdTkNavigation).First(m => m.Id == id);
			var DangNhap = collection["TenDangNhap"];
			var MatKhau = collection["MatKhau"];


			if (string.IsNullOrEmpty(DangNhap))
			{
				ViewData["Error"] = "Don't empty!";
			}
			else
			{
				taikhoan.MatKhau = MatKhau;
				TryUpdateModelAsync(taikhoan);
				_context.SaveChanges();
				return RedirectToAction("DanhSachTaiKhoan");
			}
			return this.TuyChonTaiKhoan(id);
		}
        [AdminAuthorize(MaChucNang = 23)]
        public IActionResult ResetPassword(int id)
		{
			var taikhoan = _context.TaiKhoans.Include(s => s.IdTkNavigation).First(m => m.Id == id);
			return View(taikhoan);

		}

		[HttpPost]
        [AdminAuthorize(MaChucNang = 23)]
        public IActionResult ResetPassword(int id, string email)
		{
			var user = _context.TaiKhoans.Include(s => s.IdTkNavigation).FirstOrDefault(u => u.Id == id);

			if (user != null)
			{
				string newPassword = GeneratePassword();

				user.MatKhau = DAO.GetMd5Hash(newPassword);
				_context.SaveChanges();

				// Tạo URL chứa địa chỉ email dưới dạng tham số
				string resetLink = Url.Action("DoiMatKhau", "Account", new { email = user.IdTkNavigation.Email }, protocol: Request.Scheme);
				var body = $"Vui lòng nhấp vào liên kết sau để đặt lại mật khẩu: {resetLink}";
				SendPasswordResetEmail(user.IdTkNavigation.Email, "Đặt lại mật khẩu", $"Mật khẩu mới của bạn là: {newPassword} {body}");
				return RedirectToAction("SuccessSendPassword", "RoleError");

			}
			return this.ResetPassword(id);

		}
		private string GeneratePassword()
		{

			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			var random = new Random();
			var newPassword = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
			return newPassword;
		}

		// Hàm để gửi email chứa mật khẩu mới
		private void SendPasswordResetEmail(string to, string subject, string body)
		{
			var fromAddress = new MailAddress("kikyoutnt33@gmail.com", "KDKMenShop");
			var toAddress = new MailAddress(to);
			const string fromPassword = "a m s j h a p u g c t b v s v a";

			var smtp = new SmtpClient
			{
				Host = "smtp.gmail.com", // máy chủ gmail
				Port = 587, // cổng mặc định 
				EnableSsl = true, // kích hoạt giao thức mật mã hóa(SSL,TLS) gửi gmail
				DeliveryMethod = SmtpDeliveryMethod.Network, //gửi gmail qua mạng
				UseDefaultCredentials = false,//cho biết thông tin đăng nhập được sử dụng bởi Credentials
				Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
			};

			using (var message = new MailMessage(fromAddress, toAddress)
			{
				Subject = subject,
				Body = body
			})
			{
				smtp.Send(message);
			}
		}

		public IActionResult DoiMatKhau(string email)
		{
			ViewBag.Email = email;

			return View();

		}
		[HttpPost]
		public IActionResult DoiMatKhau(string email, string newPassword, string confirmNewPassword, string returnUrl)
		{
			if (newPassword != confirmNewPassword)
			{
				ViewBag.ChangePasswordSuccess = false;
				ViewBag.ErrorMessage = "Mật khẩu mới và mật khẩu xác nhận không khớp.";
				return View();
			}

			// Tìm tài khoản với địa chỉ email được cung cấp
			var user = _context.TaiKhoans.FirstOrDefault(u => u.IdTkNavigation.Email == email);

			if (user != null)
			{
				string hashedPassword = DAO.GetMd5Hash(newPassword);

				user.MatKhau = hashedPassword;
				_context.SaveChanges();

				ViewBag.ChangePasswordSuccess = true;

				if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
				{
					return Redirect(returnUrl);
				}
				return View();
			}
			else
			{
				ViewBag.ChangePasswordSuccess = false;
				ViewBag.ErrorMessage = "Không tìm thấy tài khoản với địa chỉ email đã cung cấp.";
				return View();
			}
		}









		
	}
}
