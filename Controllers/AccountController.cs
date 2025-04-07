using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using KDKMenShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.Net.WebRequestMethods;
using System.Net.Mail;
using System.Net;
using System.Xml.Linq;
using Facebook;
using TweetSharp;
using System.Diagnostics.Tracing;
using KDKMenShop.Models.Model;
using KDKMenShop.Models.DAO;
using System.Text.RegularExpressions;

namespace KDKMenShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly ThoiTrangNamKDKContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(ThoiTrangNamKDKContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: /Account
        public IActionResult Index()
        {
            // Truyền số lượng mục trong giỏ hàng qua ViewBag để sử dụng trong view
            ViewBag.CartItemCount = HttpContext.Session.GetInt32("CartItemCount");
            ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
            ViewBag.ide = HttpContext.Session.GetInt32("id");
            return View();
        }
        // Hàm để tạo otp
        private string GenerateOTP()
        {
            Random random = new Random();
            int otp = random.Next(100000, 1000000); // Sinh số ngẫu nhiên từ 100000 đến 999999
            return otp.ToString();
        }
        // Hàm để tạo mật khẩu ngẫu nhiên
        private string GeneratePassword()
        {
            // Viết logic để tạo mật khẩu ngẫu nhiên, có thể sử dụng thư viện hoặc tạo ngẫu nhiên dựa trên các ký tự được chọn
            // Ví dụ:
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var newPassword = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
            return newPassword;
        }

        // Hàm để gửi email chứa mật khẩu mới
        private async Task SendtoEmail(string to, string subject, string body)
        {
            var fromAddress = new MailAddress("kikyoutnt33@gmail.com", "KDK Men Shop");
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
				await Task.Run(() => smtp.Send(message)); // Gửi email trong nền
			}
		}
        
        
		// GET: /Account/DangKy
		public IActionResult DangKy()
		{
			//gán giá trị 0 cho giỏ hàng và tổng tiền
			ViewBag.CartItemCount = 0;
			ViewBag.CartItemSum = 0;
			//gán giá trị 0 cho danh sách yêu thích
			ViewBag.WishlistItemCount = 0;
			if (HttpContext.Session.GetString("TaiKhoan") != null)
			{
				return RedirectToAction("Index", "Home");
			}
			return View();
		}
        // POST: /Account/DangKy
        [HttpPost]
        public IActionResult DangKy(AccountModel user)
        {
            if (ModelState.IsValid)
            {
                ViewBag.CartItemCount = 0;
                ViewBag.CartItemSum = 0;
                ViewBag.WishlistItemCount = 0;

                // Tạo mã OTP
                var otp = GenerateOTP();
                user.loaiTK = "User";

                // Kiểm tra xem email, tên đăng nhập, và số điện thoại có tồn tại không bằng 1 truy vấn duy nhất
                var existingInfo = _context.ThongTinCaNhans
                    .Where(t => t.Email == user.Email || t.Sdt == user.SDT)
                    .Select(t => new { t.Email, t.Sdt })
                    .FirstOrDefault();

                var existingUserName = _context.TaiKhoans
                    .Where(t => t.TenDangNhap == user.TaiKhoan)
                    .Select(t => t.TenDangNhap)
                    .FirstOrDefault();

                if (existingInfo != null)
                {
                    if (existingInfo.Email == user.Email)
                    {
                        ViewBag.EmailExists = true; // Email đã được đăng ký
                    }
                    if (existingInfo.Sdt == user.SDT)
                    {
                        ViewBag.PhoneExists = true; // Số điện thoại đã được đăng ký
                    }
                    return View();
                }

                if (existingUserName != null)
                {
                    ViewBag.UserNameExists = true; // Tên đăng nhập đã được sử dụng
                    return View();
                }

                // Tạo và lưu thông tin cá nhân
                var newThongTinCaNhan = new ThongTinCaNhan
                {
                    HoTen = user.Name,
                    Email = user.Email,
                    Sdt = user.SDT,
                    DiaChi = user.Diachi,
                    IsEmailConfirmed = false,
                    TrangThai = "Mở",
                    ConfirmationCode = int.Parse(otp)
                };

                _context.ThongTinCaNhans.Add(newThongTinCaNhan);
                _context.SaveChanges(); // Lưu vào CSDL để có IdTk mới

                // Lấy IdTk vừa tạo
                int newIdTK = newThongTinCaNhan.IdTk;
                HttpContext.Session.SetInt32("NewIdTK", newIdTK);

                // Tạo và lưu tài khoản
                var newUser = new TaiKhoan
                {
                    TenDangNhap = user.TaiKhoan,
                    MatKhau = MaHoaPassword.GetMd5Hash(user.Password),
                    LoaiTk = user.loaiTK,
                    IdTk = newIdTK
                };

                _context.TaiKhoans.Add(newUser);
                _context.SaveChanges();

                // Gửi mã OTP đến email của người dùng (thực hiện bất đồng bộ)
                string confirmationLink = Url.Action("XacNhanEmail", "Account", new { confirmationCode = otp, idtk = newIdTK }, protocol: Request.Scheme);
                string emailContent = $"Xin chào {user.Name}, Vui lòng xác nhận đăng ký bằng cách nhấp vào {confirmationLink}. Mã OTP của bạn là: {otp}";

                Task.Run(() => SendtoEmail(user.Email, "Xác nhận đăng ký", emailContent));

                // Chuyển hướng đến trang xác nhận email
                return RedirectToAction("XacNhanEmail", new { confirmationCode = otp, idtk = newIdTK });
            }
            return View(user);
        }
        //xác nhận email
        public IActionResult XacNhanEmail()
        {
            //gán giá trị 0 cho giỏ hàng và tổng tiền
            ViewBag.CartItemCount = 0;
            HttpContext.Session.SetInt32("CartItemCount", 0);
            ViewBag.CartItemSum = 0;
            HttpContext.Session.SetInt32("CartItemSum", 0);
            //gán giá trị 0 cho danh sách yêu thích
            ViewBag.WishlistItemCount = 0;
            HttpContext.Session.SetInt32("WishlistItemCount", 0);
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (HttpContext.Session.GetString("TaiKhoan") != null)
            {
                // Nếu đã đăng nhập, chuyển hướng đến trang chính
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public IActionResult XacNhanEmail(int confirmationCode)
        {

            int newIdTK = HttpContext.Session.GetInt32("NewIdTK") ?? 0;
            
            // Tìm thông tin cá nhân dựa trên mã xác nhận
            var thongTinCaNhan = _context.ThongTinCaNhans.FirstOrDefault(t =>  t.ConfirmationCode == confirmationCode && t.IdTk == newIdTK);
            ViewBag.NewIdTK = newIdTK;
            if (thongTinCaNhan != null)
            {
                // Cập nhật trạng thái xác nhận email thành true
                thongTinCaNhan.IsEmailConfirmed = true;
                _context.SaveChanges();

                // Hiển thị thông báo xác nhận thành công và chuyển hướng đến trang đăng nhập
                return RedirectToAction("DangNhap", "Account");
            }
            else
            {
                // Hiển thị thông báo xác nhận thất bại
                ViewBag.ConfirmationSuccess = false;
                return View();
            }
        }
        //Lấy lại mật khẩu 
        [NonEvent]
        public ActionResult LayLaiMatKhau()
        {
            UpdateCartInfo();
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (HttpContext.Session.GetString("TaiKhoan") != null)
            {
                // Nếu đã đăng nhập, chuyển hướng đến trang chính
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public ActionResult LayLaiMatKhau(string email)
        {
            UpdateCartInfo();
            // Tìm tài khoản với địa chỉ email được cung cấp
            var user = _context.TaiKhoans.FirstOrDefault(u => u.IdTkNavigation.Email == email);
                
            if (user != null)
            {
                // Tạo mật khẩu mới
                var newPassword = GeneratePassword(); // Viết hàm GeneratePassword để tạo mật khẩu mới
                string MaHoaMatKhauMoi = MaHoaPassword.GetMd5Hash(newPassword);
                // Cập nhật mật khẩu mới cho tài khoản
                user.MatKhau = MaHoaMatKhauMoi;
                _context.SaveChanges();

                // Tạo URL chứa địa chỉ email dưới dạng tham số
                string resetLink = Url.Action("DoiMatKhau", "Account", new { email = email }, protocol: Request.Scheme);
                var body = $"Vui lòng nhấp vào liên kết sau để đặt lại mật khẩu: {resetLink}";
                Task.Run(()=>SendtoEmail(email, "Đặt lại mật khẩu", $"Mật khẩu mới của bạn là: {newPassword} {body}"));

                // Thông báo cho người dùng rằng mật khẩu đã được gửi lại qua email
                ViewBag.ResetPasswordSuccess = true;
            }

            else
            {
                // Thông báo cho người dùng rằng địa chỉ email không tồn tại trong hệ thống
                ViewBag.ResetPasswordSuccess = false;
            }

            return View();
        }

        public ActionResult DoiMatKhau(string email)
        {
            UpdateCartInfo();
            // Truyền địa chỉ email qua ViewBag để sử dụng trong view
            ViewBag.Email = email;
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (HttpContext.Session.GetString("TaiKhoan") != null)
            {
                // Nếu đã đăng nhập, chuyển hướng đến trang chính
                return RedirectToAction("Index", "Home");
            }
            return View();

        }
        [HttpPost]
        public ActionResult DoiMatKhau(string email, string newPassword, string confirmNewPassword, string returnUrl)
        {
            UpdateCartInfo();
            // Kiểm tra mật khẩu mới và mật khẩu được xác nhận
            if (newPassword != confirmNewPassword)
            {
                // Nếu không khớp, thông báo lỗi và trả về view
                ViewBag.ChangePasswordSuccess = false;
                ViewBag.ErrorMessage = "Mật khẩu mới và mật khẩu xác nhận không khớp.";
                return View();
            }

            // Tìm tài khoản với địa chỉ email được cung cấp
            var user = _context.TaiKhoans.FirstOrDefault(u => u.IdTkNavigation.Email == email);

            if (user != null)
            {
                // Mã hóa mật khẩu mới
                string hashedPassword = MaHoaPassword.GetMd5Hash(newPassword);

				// Cập nhật mật khẩu mới cho tài khoản
				user.MatKhau = hashedPassword;
                _context.SaveChanges();

                // Thông báo cho người dùng rằng mật khẩu đã được đổi thành công
                ViewBag.ChangePasswordSuccess = true;

                // Kiểm tra và chuyển hướng nếu có đường dẫn trả về hợp lệ và IsLocalUrl có phải là đường dẫn cục bộ hay ko
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Nếu không có đường dẫn trả về hoặc đường dẫn không hợp lệ, trả về view
                return View();
            }
            else
            {
                // Thông báo cho người dùng rằng không tìm thấy tài khoản với địa chỉ email cung cấp
                ViewBag.ChangePasswordSuccess = false;
                ViewBag.ErrorMessage = "Không tìm thấy tài khoản với địa chỉ email đã cung cấp.";
                return View();
            }
        }
        //GET: /Account/DangNhap
        public IActionResult DangNhap()
        {
            //gán giá trị 0 cho giỏ hàng và tổng tiền
            ViewBag.CartItemCount = 0;
            HttpContext.Session.SetInt32("CartItemCount", 0);
            ViewBag.CartItemSum = 0;
            HttpContext.Session.SetInt32("CartItemSum", 0);
            //gán giá trị 0 cho danh sách yêu thích
            ViewBag.WishlistItemCount = 0;
            HttpContext.Session.SetInt32("WishlistItemCount", 0);

            if (HttpContext.Session.GetString("TaiKhoan") != null)
            {
                ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
                ViewBag.ide = HttpContext.Session.GetInt32("id");
                return RedirectToAction("Index", "Account");

            }
            return View();
        }

        // POST: /Account/DangNhap
        [HttpPost]
        public IActionResult DangNhap(TaiKhoan user)
        {
            ViewBag.CartItemCount = 0;
            ViewBag.WishlistItemCount = 0;
            ViewBag.CartItemSum = 0;

            var taikhoanForm = user.TenDangNhap;
            var matkhauForm = MaHoaPassword.GetMd5Hash(user.MatKhau);// Mã hóa mật khẩu nhập vào;

            var userCheck = _context.TaiKhoans
                                        .Where(u => u.TenDangNhap.Equals(taikhoanForm) && u.MatKhau.Equals(matkhauForm))
                                        .Select(u => new
                                        {
                                            u.Id,
                                            u.IdTk,
                                            u.TenDangNhap,
                                            ThongTinCaNhan = _context.ThongTinCaNhans.FirstOrDefault(t => t.IdTk == u.IdTk)
                                        })
                                        .SingleOrDefault();
            if (userCheck != null && userCheck.ThongTinCaNhan != null)
            {
                //if (UserCheck.LoaiTk.Trim().ToLower() == "admin")
                //{
                //    TempData["error1"] = "admin k duoc dang nhap.";
                //    return View();
                //}

                var currentTime = DateTime.Now;
				var thongTinCaNhan = _context.ThongTinCaNhans.FirstOrDefault(t => t.IdTk == userCheck.IdTk);
                if (thongTinCaNhan != null && thongTinCaNhan.IsEmailConfirmed == true && thongTinCaNhan.TrangThai != "Khóa")
                {
                    // Lưu thông tin tài khoản vào Session
                    HttpContext.Session.SetString("TaiKhoan", userCheck.TenDangNhap);
                    HttpContext.Session.SetInt32("id", userCheck.Id);
                    HttpContext.Session.SetInt32("idTK", userCheck.IdTk);
                    return RedirectToAction("Index", "Home");
                }
                else if(currentTime > thongTinCaNhan.NgayKetThuc)
                {
                    thongTinCaNhan.TrangThai = "Mở";

                    _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                    HttpContext.Session.SetString("TaiKhoan", userCheck.TenDangNhap.ToString());
                    HttpContext.Session.SetInt32("id", userCheck.Id);
                    HttpContext.Session.SetInt32("idTK", userCheck.IdTk);
                    return RedirectToAction("Index", "Home");
                }
                else if (currentTime >= thongTinCaNhan.NgayBatDau && currentTime <= thongTinCaNhan.NgayKetThuc && 
                            thongTinCaNhan != null && thongTinCaNhan.TrangThai == "Khóa")
                {
					var remainingTime = thongTinCaNhan.NgayKetThuc - DateTime.Now;
                    ViewBag.RemainingTime = remainingTime;
                    // Nếu đã qua thời gian khóa, đặt trạng thái thành "Mở"
                    
                    // Đăng nhập thất bại vì tài khoản đang bị khóa
                    ViewBag.LoginFail1 =  "Tài khoản đã bị khóa từ " + thongTinCaNhan.NgayBatDau?.ToString("dd/MM/yyyy") + " đến " + thongTinCaNhan.NgayKetThuc?.ToString("dd/MM/yyyy");
					return View("DangNhap");
                }
                else
                {
                    // Đăng nhập thất bại vì email chưa được xác nhận hoặc thông tin cá nhân không tồn tại
                    ViewBag.LoginFail1 = "Tài khoản chưa được xác nhận qua email hoặc không tồn tại.";
                    return View("DangNhap");
                }


            }
            else
            {
                // Đăng nhập thất bại, bạn có thể xử lý thông báo lỗi ở đây hoặc chuyển hướng người dùng đến trang đăng nhập lại.
                ViewBag.LoginFail1 = "Tài khoản hoặc mật khẩu không chính xác.";
                return View("DangNhap");
            }
        }
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("TaiKhoan");

            HttpContext.Session.Remove("id");

            return RedirectToAction("DangNhap", "Account");
        }
        [HttpGet]
        public IActionResult DangNhapGoogle()
        {
            string redirectUri = Url.Action("GoogleCallback", "Account", null, Request.Scheme);

            var clientSecrets = new ClientSecrets
            {
                ClientId = _configuration["Google:ClientId"],
                ClientSecret = _configuration["Google:ClientSecret"]
            };

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = clientSecrets,
                Scopes = new[] { Oauth2Service.Scope.UserinfoEmail, Oauth2Service.Scope.UserinfoProfile }
            });

            var uri = flow.CreateAuthorizationCodeRequest(redirectUri);
            uri.ResponseType = "code";

            return Redirect(uri.Build().ToString());
        }


        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string code)
        {
            string redirectUri = Url.Action("GoogleCallback", "Account", null, Request.Scheme);

            var clientSecrets = new ClientSecrets
            {
                ClientId = _configuration["Google:ClientId"],
                ClientSecret = _configuration["Google:ClientSecret"]
            };

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = clientSecrets,
                Scopes = new[] { Oauth2Service.Scope.UserinfoEmail, Oauth2Service.Scope.UserinfoProfile }
            });

            var token = await flow.ExchangeCodeForTokenAsync("user", code, redirectUri, CancellationToken.None);

            var credential = new UserCredential(flow, "user", token);

            var service = new Oauth2Service(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            var userinfo = await service.Userinfo.Get().ExecuteAsync();

            string email = userinfo.Email;
            string firstName = userinfo.GivenName;
            string lastName = userinfo.FamilyName;
            // Kiểm tra xem thông tin từ Google đã tồn tại trong CSDL hay không
            var existingUser = _context.ThongTinCaNhans.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                // Tài khoản đã tồn tại trong CSDL, đăng nhập và chuyển hướng đến trang chính
                // Kiểm tra trạng thái tài khoản
                if (existingUser.TrangThai == "Khóa")
                {
                    // Nếu tài khoản đã bị khóa, thông báo và chuyển hướng người dùng đến trang thông báo
                    TempData["ErrorMessage"] = "Tài khoản của bạn đã bị khóa.";
                    return RedirectToAction("Index", "Error");
                }
                // Lấy thông tin tài khoản liên quan từ bảng TaiKhoan
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(tk => tk.IdTk == existingUser.IdTk);

                if (taiKhoan != null)
                {
                    // Lấy TenDangNhap từ đối tượng TaiKhoan và lưu vào session
                    HttpContext.Session.SetString("TaiKhoan", taiKhoan.TenDangNhap.ToString());
                    HttpContext.Session.SetInt32("id", taiKhoan.Id);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Xử lý khi không tìm thấy thông tin tài khoản liên quan
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản.";
                    return RedirectToAction("Index", "Error");
                }

            }
            else
            {
                AccountModel user = new AccountModel();
                var newThongTinCaNhan = new ThongTinCaNhan
                {
                    IdTk = user.IdTK,
                    HoTen = firstName + "" + lastName,
                    Email = email,
                    Sdt = null, // Số điện thoại từ Facebook
                    DiaChi = null,
                    TrangThai = "Mở",
                    IsEmailConfirmed = true,
                };
                _context.ThongTinCaNhans.Add(newThongTinCaNhan);
                _context.SaveChanges();


                string inputPassword = "123"; // Mật khẩu cần mã hóa
                string hashedPassword = MaHoaPassword.GetMd5Hash(inputPassword); // Mã hóa mật khẩu
                var newTaiKhoan = new TaiKhoan
                {
                    TenDangNhap = email,
                    MatKhau = hashedPassword,
                    LoaiTk = "User",
                    IdTk = newThongTinCaNhan.IdTk
                };
                _context.TaiKhoans.Add(newTaiKhoan);
                _context.SaveChanges();
                // Lưu thông tin tài khoản vào Session
                HttpContext.Session.SetString("TaiKhoan", newTaiKhoan.TenDangNhap.ToString());
                HttpContext.Session.SetInt32("id", newTaiKhoan.Id);
                // Điều hướng đến trang chính của ứng dụng sau khi xử lý thành công
                return RedirectToAction("Index", "Home");
            }
            // Redirect based on existing user check

        }


        // Dang nhap Facebook
        public IActionResult DangNhapFacebook()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Url.Content("~/")}";
            var redirectUrl = Url.Action("FacebookCallback", "Account", null, Request.Scheme);

            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = _configuration["Facebook:AppId"],
                client_secret = _configuration["Facebook:AppSecret"],
                redirect_uri = redirectUrl,
                response_type = "code",
                scope = "email"
            });

            return Redirect(loginUrl.AbsoluteUri);
        }

        public IActionResult FacebookCallback(string code)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Url.Content("~/")}";

            var fb = new FacebookClient();
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = _configuration["Facebook:AppId"],
                client_secret = _configuration["Facebook:AppSecret"],
                redirect_uri = baseUrl + "Account/FacebookCallback",
                scope = "email",
                code = code
            });

            var accessToken = result.access_token;
            if (!string.IsNullOrEmpty(accessToken))
            {
                fb.AccessToken = accessToken;
                dynamic me = fb.Get("me?fields=first_name,middle_name,last_name,id,email");
                string email = me.email;
                string userName = me.email;
                string firstname = me.first_name;
                string middlename = me.middle_name;
                string lastname = me.last_name;
                // Kiểm tra xem thông tin từ Facebook đã tồn tại trong CSDL hay không
                var existingUser = _context.ThongTinCaNhans.FirstOrDefault(u => u.Email == email);
                if (existingUser != null)
                {
                    // Tài khoản đã tồn tại trong CSDL, đăng nhập và chuyển hướng đến trang chính
                    // Kiểm tra trạng thái tài khoản
                    if (existingUser.TrangThai == "Khóa")
                    {
                        // Nếu tài khoản đã bị khóa, thông báo và chuyển hướng người dùng đến trang thông báo
                        TempData["ErrorMessage"] = "Tài khoản của bạn đã bị khóa.";
                        return RedirectToAction("Index", "Error");
                    }
                    // Lấy thông tin tài khoản liên quan từ bảng TaiKhoan
                    var taiKhoan = _context.TaiKhoans.FirstOrDefault(tk => tk.IdTk == existingUser.IdTk);

                    if (taiKhoan != null)
                    {
                        // Lấy TenDangNhap từ đối tượng TaiKhoan và lưu vào session
                        HttpContext.Session.SetString("TaiKhoan", taiKhoan.TenDangNhap.ToString());
                        HttpContext.Session.SetInt32("id", taiKhoan.Id);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Xử lý khi không tìm thấy thông tin tài khoản liên quan
                        TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản.";
                        return RedirectToAction("Index", "Error");
                    }
                }

                else
                {
                    AccountModel user = new AccountModel();
                    var newThongTinCaNhan = new ThongTinCaNhan
                    {
                        IdTk = user.IdTK,
                        HoTen = firstname + middlename + lastname,
                        Email = email,
                        Sdt = null, // Số điện thoại từ Facebook
                        DiaChi = null,
                        TrangThai = "Mở",
                        IsEmailConfirmed = true,
                    };
                    _context.ThongTinCaNhans.Add(newThongTinCaNhan);
                    _context.SaveChanges();

                    string inputPassword = "123"; // Mật khẩu cần mã hóa
                    string hashedPassword = MaHoaPassword.GetMd5Hash(inputPassword);// Mã hóa mật khẩu
                    var newTaiKhoan = new TaiKhoan
                    {
                        TenDangNhap = userName,
                        MatKhau = hashedPassword,
                        LoaiTk = "User",
                        IdTk = newThongTinCaNhan.IdTk
                    };
                    _context.TaiKhoans.Add(newTaiKhoan);
                    _context.SaveChanges();
                    // Lưu thông tin tài khoản vào Session
                    HttpContext.Session.SetString("TaiKhoan", newTaiKhoan.TenDangNhap.ToString());
                    HttpContext.Session.SetInt32("id", newTaiKhoan.Id);
                    // Điều hướng đến trang chính của ứng dụng sau khi xử lý thành công
                    return RedirectToAction("Index", "Home");
                }
            }

            // Điều hướng đến trang chính của ứng dụng nếu xử lý thất bại
            return Redirect("/");
        }
        //ĐĂNG NHẬP TWITTER

        private string ConsumerKey = "bmGM28e5TUeXkqruqzYjhYbYo";
        private string ConsumerSecret = "8KGTxE9Rxn1JRGETLTI6vGx4S8M2QuDWwR0xNLlQrxe2aR6JbQ";
        private string CallbackUrl = "http://localhost:5208/Account/TwitterCallBack";

        public IActionResult DangNhapTwitter()
        {
            var service = new TwitterService(ConsumerKey, ConsumerSecret);
            var requestToken = service.GetRequestToken(CallbackUrl);

            var uri = service.GetAuthorizationUri(requestToken);
            return Redirect(uri.ToString());
        }

        public IActionResult TwitterCallback(string oauth_token, string oauth_verifier)
        {
            if (string.IsNullOrEmpty(oauth_token) || string.IsNullOrEmpty(oauth_verifier))
            {
                // Người dùng đã hủy bỏ xác thực, chuyển hướng về trang đăng nhập
                return RedirectToAction("DangNhap", "Account");
            }
            var service = new TwitterService(ConsumerKey, ConsumerSecret);

            //sử dụng mã oauth_token để khởi tạo đối tượng OAuthRequestToken
            OAuthRequestToken requestToken = new OAuthRequestToken { Token = oauth_token };

            // AccessToken này được sử dụng để xác thực yêu cầu đến API của Twitter dưới danh tính của người dùng
            OAuthAccessToken accessToken = service.GetAccessToken(requestToken, oauth_verifier);

            //sau khi xác thực -> gửi yêu cầu để xác minh danh tính
            service.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);

            // Lấy thông tin người dùng từ Twitter
            TwitterUser TU = service.VerifyCredentials(new VerifyCredentialsOptions());

            // Xử lý thông tin người dùng ở đây, ví dụ:
            string screenName = TU.ScreenName;
            string fullName = TU.Name;
            // Kiểm tra xem thông tin từ Twitter đã tồn tại trong CSDL hay không
            var existingUser = _context.ThongTinCaNhans.FirstOrDefault(u => u.Email == screenName);
            if (existingUser != null)
            {
                // Tài khoản đã tồn tại trong CSDL, đăng nhập và chuyển hướng đến trang chính
                // Kiểm tra trạng thái tài khoản
                if (existingUser.TrangThai == "Khóa")
                {
                    // Nếu tài khoản đã bị khóa, thông báo và chuyển hướng người dùng đến trang thông báo
                    TempData["ErrorMessage"] = "Tài khoản của bạn đã bị khóa.";
                    return RedirectToAction("Index", "Error");
                }
                // Lấy thông tin tài khoản liên quan từ bảng TaiKhoan
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(tk => tk.IdTk == existingUser.IdTk);

                if (taiKhoan != null)
                {
                    // Lấy TenDangNhap từ đối tượng TaiKhoan và lưu vào session
                    HttpContext.Session.SetString("TaiKhoan", taiKhoan.TenDangNhap.ToString());
                    HttpContext.Session.SetInt32("id", taiKhoan.Id);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Xử lý khi không tìm thấy thông tin tài khoản liên quan
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản.";
                    return RedirectToAction("Index", "Error");
                }
            }
            else
            {
                AccountModel user = new AccountModel();
                var newThongTinCaNhan = new ThongTinCaNhan
                {
                    IdTk = user.IdTK,
                    HoTen = fullName,
                    Email = screenName,
                    Sdt = null, // Số điện thoại từ Facebook
                    DiaChi = null,
                    TrangThai = "Mở",
                    IsEmailConfirmed = true,
                };
                _context.ThongTinCaNhans.Add(newThongTinCaNhan);
                _context.SaveChanges();

                 string inputPassword = "123"; // Mật khẩu cần mã hóa
                string hashedPassword = MaHoaPassword.GetMd5Hash(inputPassword); // Mã hóa mật khẩu
                var newTaiKhoan = new TaiKhoan
                {
                    TenDangNhap = screenName,
                    MatKhau = hashedPassword,
                    LoaiTk = "User",
                    IdTk = newThongTinCaNhan.IdTk
                };
                _context.TaiKhoans.Add(newTaiKhoan);
                _context.SaveChanges();
                // Lưu thông tin tài khoản vào Session
                HttpContext.Session.SetString("TaiKhoan", newTaiKhoan.TenDangNhap.ToString());
                HttpContext.Session.SetInt32("id", newTaiKhoan.Id);
                // Điều hướng đến trang chính của ứng dụng sau khi xử lý thành công
                return RedirectToAction("Index", "Home");
            }
        }
        public IActionResult ThongTinCaNhan()
        {


            //Lấy ID người dùng từ session
            int? userIdTK = HttpContext.Session.GetInt32("idTK");
            int? userId = HttpContext.Session.GetInt32("id");
            ViewBag.cc = HttpContext.Session.GetString("TaiKhoan");
            ViewBag.ide = HttpContext.Session.GetInt32("id");


            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (userId == null || string.IsNullOrEmpty(HttpContext.Session.GetString("TaiKhoan")))
            {
                return RedirectToAction("DangNhap", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
            }

            UpdateCartInfo();
            var thongTinCaNhan = _context.ThongTinCaNhans.FirstOrDefault(t => t.IdTk == userIdTK);
            if (thongTinCaNhan == null)
            {


                return RedirectToAction("Error", "Home");
            }
            // Map the user information to the view model
            var viewModel = new AccountModel
            {
                Name = thongTinCaNhan.HoTen,
                Email = thongTinCaNhan.Email,
                Diachi = thongTinCaNhan.DiaChi,
                SDT = thongTinCaNhan.Sdt != null ? thongTinCaNhan.Sdt.ToString() : null
            };

            // Pass the view model to the view
            return View(viewModel);
        }
        [HttpPost]
        public IActionResult CapNhatThongTin(AccountModel model)
        {
            // Kiểm tra xem dữ liệu nhập vào có hợp lệ không
            if (ModelState.IsValid)
            {
                // Lấy ID người dùng từ session
                int? userIdTK = HttpContext.Session.GetInt32("idTK");

                // Lấy thông tin người dùng từ database dựa trên userIdTK
                var thongTinCaNhan = _context.ThongTinCaNhans.FirstOrDefault(t => t.IdTk == userIdTK);

                // Kiểm tra xem người dùng có tồn tại hay không
                if (thongTinCaNhan != null)
                {
                    // Cập nhật thông tin người dùng với dữ liệu mới từ model
                    thongTinCaNhan.HoTen = model.Name;
                    thongTinCaNhan.Email = model.Email;
                    thongTinCaNhan.DiaChi = model.Diachi;
                    
                    thongTinCaNhan.Sdt = !string.IsNullOrEmpty(model.SDT) ? model.SDT.ToString() : null;

                    // Lưu các thay đổi vào database
                    _context.SaveChanges();

                    // Redirect người dùng về trang thông tin cá nhân sau khi cập nhật thành công
                    // Tạo HTML mới chứa thông tin đã cập nhật
                    // Tạo chuỗi HTML mới chứa thông tin đã cập nhật
                    string updatedHtml = $@"
                    <div class='bg-secondary d-lg-inline-block py-1-9 px-1-9 px-sm-6 mb-1-9 rounded'>
                        <h3 class='h2 text-white mb-0' id='name'>{thongTinCaNhan.HoTen}</h3>
                    </div>
                    <ul class='list-unstyled mb-1-9' id='userInfo'>
                        <li class='mb-2 mb-xl-3 display-28'><span class='display-26 text-secondary me-2 font-weight-600'>Email:</span> <span id='email'>{thongTinCaNhan.Email}</span></li>
                        <li class='mb-2 mb-xl-3 display-28'><span class='display-26 text-secondary me-2 font-weight-600'>Địa Chỉ:</span> <span id='diachi'>{thongTinCaNhan.DiaChi}</span></li>
                        <li class='mb-2 mb-xl-3 display-28'><span class='display-26 text-secondary me-2 font-weight-600'>Số Điện Thoại:</span> <span id='sdt'>{thongTinCaNhan.Sdt}</span></li>
                        <li class='mb-2 mb-xl-3 display-28'><span class='display-26 text-secondary me-2 font-weight-600'>Thành Viên:</span> Chưa Có</li>
                    </ul>
                    <div>
                    <button type=""button"" class=""btn btn-sm btn-primary"" data-toggle=""modal"" data-target=""#editModal"">
                        Sửa
                     </button>
                     </div>";
                    // Trả về chuỗi HTML mới
                    return Content(updatedHtml);




                }
                else
                {
                    // Nếu không tìm thấy thông tin cá nhân, chuyển hướng đến trang lỗi
                    return RedirectToAction("Error", "Home");
                }
            }

            // Nếu dữ liệu không hợp lệ, hiển thị lại form với thông báo lỗi
            return View("ThongTinCaNhan", model);
        }
        public void UpdateCartInfo()
        {
            int? userId = HttpContext.Session.GetInt32("id");
            string taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            ViewBag.cc = taiKhoan;

            if (userId.HasValue)
            {
                // Get cart info in one query to reduce database hits
                var cartInfo = _context.GioHangs
                    .Where(gh => gh.Id == userId.Value)
                    .Select(gh => new
                    {
                        gh.MaGioHang,
                        CartItems = _context.ChiTietGioHangs.Where(g => g.MaGioHang == gh.MaGioHang)
                    })
                    .FirstOrDefault();

                if (cartInfo != null)
                {
                    int cartItemCount = cartInfo.CartItems.Sum(g => g.SoLuong);
                    ViewBag.CartItemCount = cartItemCount;
                    HttpContext.Session.SetInt32("CartItemCount", cartItemCount);

                    decimal cartItemSum = cartInfo.CartItems.Sum(g => g.SoLuong * g.Gia);
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

                // Wishlist info
                var wishlist = _context.Wishlists
                    .Where(w => w.Id == userId.Value)
                    .Select(w => new
                    {
                        w.WishlistId,
                        ItemCount = _context.ChiTietWishlists.Where(g => g.WishlistId == w.WishlistId).Count()
                    })
                    .FirstOrDefault();

                if (wishlist != null)
                {
                    ViewBag.WishlistItemCount = wishlist.ItemCount;
                    HttpContext.Session.SetInt32("WishlistItemCount", wishlist.ItemCount);
                }
                else
                {
                    ViewBag.WishlistItemCount = 0;
                    HttpContext.Session.SetInt32("WishlistItemCount", 0);
                }
            }
            else
            {
                ViewBag.CartItemCount = 0;
                ViewBag.CartItemSum = 0;
                ViewBag.WishlistItemCount = 0;
                HttpContext.Session.SetInt32("CartItemCount", 0);
                HttpContext.Session.SetInt32("CartItemSum", 0);
                HttpContext.Session.SetInt32("WishlistItemCount", 0);
            }

        }
    }

}
