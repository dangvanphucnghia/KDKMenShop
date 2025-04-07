using KDKMenShop.Models;
using KDKMenShop.Models.ViewModels;
using KDKMenShop.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Text;
using MoMo;
using KDKMenShop.Others;
using Newtonsoft.Json.Linq;
using System.Net;
using MessagePack.Formatters;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using KDKMenShop.Models.ProductModel;
using System.Net.Mail;
using NuGet.Protocol.Plugins;
using System.Globalization;
using KDKMenShop.Models.Model;
using Microsoft.AspNetCore.Http;
using System.Security.Policy;
namespace KDKMenShop.Controllers
{
    public class CartController : Controller
    {

        private readonly ThoiTrangNamKDKContext _context;
        private readonly IConfiguration _configuration;
        public CartController(ThoiTrangNamKDKContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public ActionResult Home()
        {
            return View();
        }
        public ActionResult CheckOut()
        {
            return View("~/Views/CheckOut/Index.cshtml");
        }
		// Hàm tạo mã giỏ hàng duy nhất
		private string GenerateUniqueCartCode(int? userId)
		{
			// Tạo mã giỏ hàng từ userId và thời gian hiện tại
			string uniqueCode = $"{userId}-{DateTime.Now.Ticks}";

			// Chuyển mã thành hash code (băm)
			int hashCode = uniqueCode.GetHashCode();

			// Chuyển hash code thành số dương để tránh số âm
			hashCode = hashCode & 0x7FFFFFFF;

			// Lấy 5 chữ số cuối cùng của hash code
			string cartCode = hashCode.ToString().Substring(Math.Max(0, hashCode.ToString().Length - 5));

			// Trả về mã giỏ hàng duy nhất
			return cartCode;
		}
        public void UpdateTTCN()
        {
			//Lấy ID người dùng từ session
			int? userId = HttpContext.Session.GetInt32("id");
			var gioHang = _context.GioHangs.Include(g => g.ChiTietGioHangs).FirstOrDefault(g => g.Id == userId);


			string cartId = gioHang?.MaGioHang;


			var chiTiet = _context.ChiTietGioHangs.FirstOrDefault(g => g.MaGioHang == cartId);

			// Lấy tên tài khoản từ session
			string taiKhoan = HttpContext.Session.GetString("TaiKhoan");
			ViewBag.cc = taiKhoan;


			// Lấy số lượng các mục trong giỏ hàng từ cơ sở dữ liệu
			int cartItemCount = (int)_context.ChiTietGioHangs
										.Where(g => g.MaGioHang == cartId)
										.Sum(g => g.SoLuong);

			//// Truyền số lượng mục trong giỏ hàng qua ViewBag để sử dụng trong view
			ViewBag.CartItemCount = cartItemCount;
			HttpContext.Session.SetInt32("CartItemCount", cartItemCount);

			
			var wishlist = _context.Wishlists.FirstOrDefault(w => w.Id == userId);
			if (wishlist != null)
			{
				// Lấy số lượng các mục trong giỏ hàng từ cơ sở dữ liệu
				string wishlistID = wishlist.WishlistId;
				int wishlistItemCount = _context.ChiTietWishlists
									.Where(g => g.WishlistId == wishlistID).Count();

				ViewBag.WishlistItemCount = wishlistItemCount;
				HttpContext.Session.SetInt32("WishlistItemCount", wishlistItemCount);
			}
			
		}
		// trang gior hàng
		public IActionResult Index()
        {

            //Lấy ID người dùng từ session
            int? userId = HttpContext.Session.GetInt32("id");

            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (userId == null || string.IsNullOrEmpty(HttpContext.Session.GetString("TaiKhoan")))
            {
                return RedirectToAction("DangNhap", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
            }

			// Lấy thông tin giỏ hàng từ cơ sở dữ liệu dựa trên ID người dùng
			var gioHang = _context.GioHangs.FirstOrDefault(g => g.Id == userId);
			string cartId = gioHang?.MaGioHang;
			var chiTiet = _context.ChiTietGioHangs.FirstOrDefault(g => g.MaGioHang == cartId);
			UpdateTTCN();


			decimal cartItemSum = (decimal)_context.ChiTietGioHangs
											.Where(g => g.MaGioHang == cartId)
											.Sum(g => g.SoLuong * g.Gia);
			ViewBag.CartItemSum = cartItemSum;
			HttpContext.Session.SetInt32("CartItemSum", (int)cartItemSum);
			// Xác định thời gian hiện tại
			DateTime currentTime = DateTime.Now;

			var cartItems = _context.ChiTietGioHangs
                .Where(g => g.MaGioHang == cartId)
                .Select(g => new CartItemModel
                {
                    ProductId = (int)g.MaSanPham,
                    Soluong = (int)g.SoLuong,
                    KichThuoc = g.KichThuoc,
                    SoLuongTon = _context.ChiTietKichThuocs.Where(sp => sp.MaSanPham == g.MaSanPham && sp.KichThuoc == g.KichThuoc).Select(sp => sp.SoLuong).FirstOrDefault(),
                    TenSP = _context.SanPhams.Where(sp => sp.MaSanPham == g.MaSanPham).Select(sp => sp.TenSanPham).FirstOrDefault(),
                    Gia = (int)g.Gia,
                    image = _context.SanPhams.Where(sp => sp.MaSanPham == g.MaSanPham).Select(sp => sp.HinhAnh).FirstOrDefault(),
                    Total = (int)g.TongTien,
                    // Lấy đường dẫn hình ảnh từ bảng SanPham
                    // Thêm các trường dữ liệu khác của sản phẩm nếu cần thiết

                }).ToList();

			// Lặp qua từng sản phẩm trong giỏ hàng để kiểm tra và cập nhật giá nếu cần
			foreach (var item in cartItems)
			{

				var sanpham = _context.SanPhams.FirstOrDefault(p => p.MaSanPham == item.ProductId);
				var khuyenMai = _context.ChiTietKhuyenMais
								.Where(ct => ct.MaSanPham == sanpham.MaSanPham)
								.Join(_context.KhuyenMais,
									  ct => ct.MaVoucher,
									  km => km.MaVoucher,
									  (ct, km) => new
									  {
										  KhuyenMai = km,
										  ChiTietKhuyenMai = ct
									  })
								.FirstOrDefault();
				item.Gia = sanpham.Gia;
				var chiTietGioHang = _context.ChiTietGioHangs.FirstOrDefault(c => c.MaGioHang == cartId && c.MaSanPham == item.ProductId);
				
				if (khuyenMai != null && KiemTraKhuyenMai(khuyenMai.KhuyenMai)) { 
					// Cập nhật giá mới của sản phẩm trong giỏ hàng

					item.Gia -= (item.Gia * khuyenMai.KhuyenMai.PhanTramGiam) / 100;
					// Cập nhật tổng tiền
					item.Total = item.Soluong * item.Gia;
					if (chiTietGioHang != null)
					{
						chiTietGioHang.Gia = item.Gia;
                        chiTietGioHang.TongTien = (int)item.Total;
						_context.ChiTietGioHangs.Update(chiTietGioHang);
						_context.SaveChanges();
					}
				}
                else
                {
					item.Gia = sanpham.Gia;
					item.Total = item.Soluong * item.Gia;
					if (chiTietGioHang != null)
					{
						chiTietGioHang.Gia = item.Gia;
						chiTietGioHang.TongTien = (int)item.Total;
						_context.ChiTietGioHangs.Update(chiTietGioHang);
						_context.SaveChanges();
					}
				}   
				// Cập nhật lại giá mới vào chi tiết giỏ hàng
			}
			// Lưu các thay đổi vào cơ sở dữ liệu
			// Tính tổng số lượng và tổng giá tiền của giỏ hàng
			decimal grandTotal = cartItems.Sum(x => x.Soluong * x.Gia);
            // Tạo đối tượng view model cho giỏ hàng
            var cartViewModel = new CartItemViewModel
            {
                CartItems = cartItems,
                GrandTotal = grandTotal,
            };
            return View(cartViewModel); // Trả về view hiển thị thông tin giỏ hàng
        }
		public async Task<IActionResult> ApplyDiscountCode(string discountCode)
		{
			int? userId = HttpContext.Session.GetInt32("id");
			// Lấy thông tin giỏ hàng của người dùng
			var gioHang = _context.GioHangs.FirstOrDefault(g => g.Id == userId);
			string cartId = gioHang.MaGioHang;

            // Lấy mã giảm giá từ bảng KhuyenMai
            var discount = await _context.KhuyenMais.FirstOrDefaultAsync(k => k.MaVoucher == discountCode);			// Kiểm tra xem discount có null không
			if (discount == null)
			{
				return Json(new { success = false, message = "Mã giảm giá không hợp lệ." });
			}
            // Kiểm tra xem mã giảm giá đã được sử dụng hay chưa
            if ((bool)discount.SoLan)
            {
                return Json(new { success = false, message = "Mã giảm giá đã được sử dụng." });
            }
            // Kiểm tra xem thời gian bắt đầu và kết thúc của discount
            if (!(discount.ThoiGianBatDau <= DateTime.Now && discount.ThoiGianKetThuc >= DateTime.Now))
			{
				return Json(new { success = false, message = "Mã Giảm Đã Hết Hạn." });
			}

			// Kiểm tra xem mã giảm giá có được áp dụng cho người dùng này không
			if (discount.Id != userId)
			{
				return Json(new { success = false, message = "Mã giảm giá không áp dụng cho người dùng này." });
			}
			// Lấy tổng giá trị ban đầu của giỏ hàng
			decimal grandTotal = (decimal)_context.ChiTietGioHangs
										   .Where(g => g.MaGioHang == cartId)
										   .Sum(g => g.SoLuong * g.Gia);
			decimal grandTotalTT = (decimal)_context.ChiTietGioHangs
											.Where(g => g.MaGioHang == cartId)
											.Sum(g => g.SoLuong * g.Gia);
			// Tính toán giá trị giảm giá
			decimal discountAmount = (grandTotal * discount.PhanTramGiam) / 100;
			decimal discountAmountTT = (grandTotalTT * discount.PhanTramGiam) / 100;
			// Áp dụng giảm giá vào grandTotal
			grandTotal -= discountAmount;
            grandTotalTT -= discountAmountTT;
            // Đảm bảo grandTotal không nhỏ hơn 0
            if (grandTotal < 0)
			{
				grandTotal =0;
			}
            //// Đánh dấu mã giảm giá đã được sử dụng
            //discount.SoLan = true;
            //_context.Update(discount);
            //await _context.SaveChangesAsync();
            // Format grandTotal as currency string with thousand separators and " VNĐ" suffix
            string formattedGrandTotal = grandTotal.ToString("###,###,### VNĐ");
            HttpContext.Session.SetString("voucherCode", discountCode.ToString());
			return Json(new { success = true, formattedGrandTotal , grandTotalTT });
		}



        private bool KiemTraKhuyenMai(KhuyenMai khuyenMai)
        {
            DateTime now = DateTime.Now;
            return now >= khuyenMai.ThoiGianBatDau && now <= khuyenMai.ThoiGianKetThuc;
        }
        public async Task<IActionResult> ThemGio(int MaSanPham, int soLuong = 1, string kichThuoc = "S")
        {
            int? userId = HttpContext.Session.GetInt32("id");


            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (userId == null || string.IsNullOrEmpty(HttpContext.Session.GetString("TaiKhoan")))
            {
                //return RedirectToAction("DangNhap", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
                return Json(new { success = false, error = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng" });
            }

            var sanpham = _context.SanPhams.FirstOrDefault(p => p.MaSanPham == MaSanPham);

            var khuyenMai = _context.ChiTietKhuyenMais
                            .Where(ct => ct.MaSanPham == sanpham.MaSanPham)
                            .Join(_context.KhuyenMais,
                                  ct => ct.MaVoucher,
                                  km => km.MaVoucher,
                                  (ct, km) => new
                                  {
                                      KhuyenMai = km,
                                      ChiTietKhuyenMai = ct
                                  })
                            .FirstOrDefault();


            int productPrice = sanpham.Gia;
            if (sanpham == null)
            {
                return NotFound();
            }

            var existingCart = _context.GioHangs.FirstOrDefault(g => g.Id == userId);
            string cartId = "";

            if (existingCart == null)
            {
                cartId = GenerateUniqueCartCode(userId);
                var newCart = new GioHang
                {
                    MaGioHang = cartId,
                    Id = (int)userId
                };
                _context.GioHangs.Add(newCart);
                _context.SaveChanges();
            }
            else
            {
                cartId = existingCart.MaGioHang;
            }
            var laysoluong = _context.ChiTietGioHangs.FirstOrDefault(c => c.MaGioHang == cartId && c.MaSanPham == MaSanPham && c.KichThuoc == kichThuoc);
            var laysoluongCT = _context.ChiTietKichThuocs.FirstOrDefault(c => c.MaSanPham == MaSanPham && c.KichThuoc == kichThuoc);
            var soluongCT = (int)laysoluongCT.SoLuong;
            int soluong = laysoluong != null ? (int)laysoluong.SoLuong : 0;

            if (soluong >= soluongCT)
            {
                return Json(new { success = false, error = "Số lượng sản phẩm trong giỏ hàng đã đạt giới hạn của kích thước đã chọn" });
            }
            var existingCartItem = _context.ChiTietGioHangs.FirstOrDefault(c => c.MaGioHang == cartId && c.MaSanPham == MaSanPham && c.KichThuoc == kichThuoc);

            if (existingCartItem == null)
            {
                
                if (khuyenMai != null && KiemTraKhuyenMai(khuyenMai.KhuyenMai))
                {
                    // Nếu có khuyến mãi, giảm giá
                    productPrice -= (productPrice * khuyenMai.KhuyenMai.PhanTramGiam) / 100;
                }
                var gioHang = new ChiTietGioHang
			    {
				    MaGioHang = cartId,
				    SoLuong = soLuong,
				    Gia = productPrice,
				    TongTien = sanpham.Gia * soLuong,
				    MaSanPham = MaSanPham,
				    KichThuoc = kichThuoc,
				    MaChiTiet = _context.ChiTietKichThuocs.Where(sp => sp.MaSanPham == MaSanPham && sp.KichThuoc == kichThuoc).Select(sp => sp.MaChiTiet).FirstOrDefault()

			    };
					_context.ChiTietGioHangs.Add(gioHang);
					await _context.SaveChangesAsync();
			}
            else
            {
                existingCartItem.SoLuong += soLuong;
                existingCartItem.TongTien += sanpham.Gia * soLuong;
                await _context.SaveChangesAsync();
            }

            // Calculate cart item count
            int cartItemCount = (int)await _context.ChiTietGioHangs
                                    .Where(g => g.MaGioHang == cartId)
                                    .SumAsync(g => g.SoLuong);
            decimal cartItemSum = (decimal)_context.ChiTietGioHangs
                                            .Where(g => g.MaGioHang == cartId)
                                            .Sum(g => g.SoLuong * g.Gia);
            return Json(new { success = true, message = "Sản phẩm đã được thêm vào giỏ hàng", cartItemCount, cartItemSum });
        }

        public async Task<IActionResult> GiamSL(int MaSanPham, string kichThuoc = "S")
        {
            int? userId = HttpContext.Session.GetInt32("id");


            // Tìm sản phẩm trong giỏ hàng
            var gioHang = _context.GioHangs.FirstOrDefault(g => g.Id == userId);


            string cartId = gioHang.MaGioHang;
            var CTgioHang = await _context.ChiTietGioHangs.FirstOrDefaultAsync(g => g.MaGioHang == cartId && g.MaSanPham == MaSanPham && g.KichThuoc == kichThuoc);
            SanPham sanpham = await _context.SanPhams.FindAsync(MaSanPham);



            if (CTgioHang != null)
            {
                if (CTgioHang.SoLuong > 1)
                {
                    CTgioHang.SoLuong--;

                    // Cập nhật số lượng sản phẩm trong cơ sở dữ liệu
                    CTgioHang.TongTien -= sanpham.Gia;


                    _context.ChiTietGioHangs.Update(CTgioHang);

                    await _context.SaveChangesAsync();
                    // Tính toán lại tổng giá trị của giỏ hàng
                    decimal grandTotal = (decimal)_context.ChiTietGioHangs
                                            .Where(g => g.MaGioHang == cartId)
                                            .Sum(g => g.SoLuong * g.Gia);
					decimal grandTotalTT = (decimal)_context.ChiTietGioHangs
											.Where(g => g.MaGioHang == cartId)
											.Sum(g => g.SoLuong * g.Gia);
					decimal productTotal = (decimal)_context.ChiTietGioHangs
                                           .Where(g => g.MaGioHang == cartId && g.MaSanPham == MaSanPham && g.KichThuoc == kichThuoc)
                                           .Sum(g => g.SoLuong * g.Gia);

                    int cartItemCount = (int)_context.ChiTietGioHangs
                                     .Where(g => g.MaGioHang == cartId)
                                    .Sum(g => g.SoLuong);
					string formattedproductTotal = productTotal.ToString("###,###,###");
					string formattedGrandTotal = grandTotal.ToString("###,###,### VNĐ");
					return Json(new { success = true, message = "Giảm số lượng sản phẩm thành công", grandTotal=formattedGrandTotal, grandTotalTT, productTotal= formattedproductTotal, cartItemCount });
                }

                else
                {
                    CTgioHang.SoLuong++;
                    //Xóa sản phẩm khỏi giỏ hàng và cơ sở dữ liệu

                    //_context.ChiTietGioHangs.Remove(CTgioHang);
                    //await _context.SaveChangesAsync();
                    //decimal grandTotal = (decimal)_context.ChiTietGioHangs
                    //                        .Where(g => g.MaGioHang == cartId)
                    //                        .Sum(g => g.SoLuong * g.Gia);
                    //decimal productTotal = (decimal)_context.ChiTietGioHangs
                    //                       .Where(g => g.MaGioHang == cartId && g.MaSanPham == MaSanPham && g.KichThuoc == kichThuoc)
                    //                       .Sum(g => g.SoLuong * g.Gia);
                    return Json(new { success = true, message = "Xóa sản phẩm khỏi giỏ hàng thành công"/*, grandTotal, productTotal*/ });
                }
            }
            else
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" });
            }
        }

        public async Task<IActionResult> TangSL(int MaSanPham, string kichThuoc = "S")
        {
            int? userId = HttpContext.Session.GetInt32("id");


            // Tìm sản phẩm trong giỏ hàng
            var gioHang = _context.GioHangs.FirstOrDefault(g => g.Id == userId);


            string cartId = gioHang.MaGioHang;
            var CTgioHang = await _context.ChiTietGioHangs.FirstOrDefaultAsync(g => g.MaGioHang == cartId && g.MaSanPham == MaSanPham && g.KichThuoc == kichThuoc);
            SanPham sanpham = await _context.SanPhams.FindAsync(MaSanPham);
            var laysoluong = _context.ChiTietGioHangs.FirstOrDefault(c => c.MaGioHang == cartId && c.MaSanPham == MaSanPham && c.KichThuoc == kichThuoc);
            var laysoluongCT = _context.ChiTietKichThuocs.FirstOrDefault(c => c.MaSanPham == MaSanPham && c.KichThuoc == kichThuoc);
            var soluongCT = (int)laysoluongCT.SoLuong;
            int soluong = laysoluong != null ? (int)laysoluong.SoLuong : 0;


            //HttpContext.Session.SetInt32("SoLuong", soluong);
            if (soluong == soluongCT)
            {
                return Json(new { success = false, error = "Số lượng sản phẩm trong giỏ hàng đã đạt giới hạn của kích thước đã chọn" });
            }
            // Lấy số lượng các mục trong giỏ hàng từ cơ sở dữ liệu


            if (CTgioHang != null)
            {
                if (CTgioHang.SoLuong >= 1)
                {
                    CTgioHang.SoLuong++;

                    // Cập nhật số lượng sản phẩm trong cơ sở dữ liệu
                    CTgioHang.TongTien += sanpham.Gia;


                    _context.ChiTietGioHangs.Update(CTgioHang);

                    await _context.SaveChangesAsync();
                    // Tính toán lại tổng giá trị của giỏ hàng
                    decimal grandTotal = (decimal)_context.ChiTietGioHangs
                                            .Where(g => g.MaGioHang == cartId)
                                            .Sum(g => g.SoLuong * g.Gia);
					decimal grandTotalTT = (decimal)_context.ChiTietGioHangs
											.Where(g => g.MaGioHang == cartId)
											.Sum(g => g.SoLuong * g.Gia);
					decimal productTotal = (decimal)_context.ChiTietGioHangs
                                           .Where(g => g.MaGioHang == cartId && g.MaSanPham == MaSanPham && g.KichThuoc == kichThuoc)
                                           .Sum(g => g.SoLuong * g.Gia);
                    int cartItemCount = (int)_context.ChiTietGioHangs
                                       .Where(g => g.MaGioHang == cartId)
                                      .Sum(g => g.SoLuong);
					string formattedproductTotal = productTotal.ToString("###,###,###");
					string formattedGrandTotal = grandTotal.ToString("###,###,### VNĐ");
					return Json(new { success = true, message = "Tăng số lượng sản phẩm thành công", grandTotal=formattedGrandTotal, grandTotalTT, productTotal= formattedproductTotal, cartItemCount });
                }


            }
            return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" });

        }
        // Phương thức để cập nhật số lượng sản phẩm trong giỏ hàng
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int newQuantity, string kichThuoc = "S")
        {

            int? userId = HttpContext.Session.GetInt32("id");


            // Tìm sản phẩm trong giỏ hàng
            var gioHang = _context.GioHangs.FirstOrDefault(g => g.Id == userId);


            string cartId = gioHang.MaGioHang;
            var CTgioHang = await _context.ChiTietGioHangs.FirstOrDefaultAsync(g => g.MaGioHang == cartId && g.MaSanPham == productId && g.KichThuoc == kichThuoc);


            var laysoluongCT = _context.ChiTietKichThuocs.FirstOrDefault(c => c.MaSanPham == productId && c.KichThuoc == kichThuoc);
            var soluongCT = (int)laysoluongCT.SoLuong;

            if (newQuantity > soluongCT)
            {
                return Json(new { success = false, soluongCT, message = "Số lượng sản phẩm trong giỏ hàng đã đạt giới hạn của kích thước đã chọn" });
            }

            if (CTgioHang != null)
            {
                // Đảm bảo newQuantity là lớn hơn hoặc bằng 1
                if (newQuantity >= 1)
                {
                    // Cập nhật số lượng sản phẩm trong giỏ hàng
                    CTgioHang.SoLuong = newQuantity;
                    CTgioHang.TongTien = newQuantity * CTgioHang.Gia;
                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();

                    // Tính toán lại tổng giá trị của giỏ hàng
                    decimal grandTotal = (decimal)_context.ChiTietGioHangs
                                             .Where(g => g.MaGioHang == cartId)
                                             .Sum(g => g.SoLuong * g.Gia);
					decimal grandTotalTT = (decimal)_context.ChiTietGioHangs
											.Where(g => g.MaGioHang == cartId)
											.Sum(g => g.SoLuong * g.Gia);
					decimal productTotal = (decimal)_context.ChiTietGioHangs
                                           .Where(g => g.MaGioHang == cartId && g.MaSanPham == productId && g.KichThuoc == kichThuoc)
                                           .Sum(g => g.SoLuong * g.Gia);
                    int cartItemCount = (int)_context.ChiTietGioHangs
                                       .Where(g => g.MaGioHang == cartId)
                                      .Sum(g => g.SoLuong);
					string formattedproductTotal = productTotal.ToString("###,###,###");
					string formattedGrandTotal = grandTotal.ToString("###,###,### VNĐ");
					// Trả về phản hồi cho yêu cầu AJAX
					return Json(new
                    {
                        success = true,
                        message = "Đã cập nhật số lượng sản phẩm trong giỏ hàng.",
						grandTotal=formattedGrandTotal,
						grandTotalTT,
						productTotal = formattedproductTotal,
                        cartItemCount
                    });
                }

                else
                {
                    // Trả về phản hồi nếu newQuantity không hợp lệ
                    return Json(new { success = false, message = "Số lượng sản phẩm phải lớn hơn hoặc bằng 1." });
                }

            }
            else
            {
                // Trả về phản hồi nếu không tìm thấy sản phẩm trong giỏ hàng
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });
            }
        }


        public async Task<IActionResult> XoaKhoiGioHang(int MaSanPham, string kichThuoc = "S")
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            int? userId = HttpContext.Session.GetInt32("id");
            var gioHang = _context.GioHangs.FirstOrDefault(g => g.Id == userId);


            string cartId = gioHang.MaGioHang;
            // Remove the item from the cart
            cart.RemoveAll(p => p.ProductId == MaSanPham && p.KichThuoc == kichThuoc);
            // Tìm sản phẩm trong giỏ hàng
            var cartItem = await _context.ChiTietGioHangs.SingleOrDefaultAsync(c => c.MaGioHang == cartId && c.MaSanPham == MaSanPham && c.KichThuoc == kichThuoc);
            if (cartItem != null)
            {
                // Xóa sản phẩm khỏi giỏ hàng
                _context.ChiTietGioHangs.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            if (cart.Count == 0)
            {
                // If the cart is empty after removing the item, remove the entire cart from the session
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                // If the cart still contains items, update the cart in the session
                HttpContext.Session.SetJson("Cart", cart);
            }

            // Calculate the grand total
            decimal grandTotal = (decimal)_context.ChiTietGioHangs
                                            .Where(g => g.MaGioHang == cartId)
                                            .Sum(g => g.SoLuong * g.Gia);
			decimal grandTotalTT = (decimal)_context.ChiTietGioHangs
									.Where(g => g.MaGioHang == cartId)
									.Sum(g => g.SoLuong * g.Gia);
			int cartItemCount = (int)_context.ChiTietGioHangs
                                       .Where(g => g.MaGioHang == cartId && g.KichThuoc == kichThuoc)
                                      .Sum(g => g.SoLuong);
            decimal cartItemSum = (decimal)_context.ChiTietGioHangs
                                            .Where(g => g.MaGioHang == cartId)
                                            .Sum(g => g.SoLuong * g.Gia);
			string formattedGrandTotal = grandTotal.ToString("###,###,### VNĐ");
			return Json(new { success = true, grandTotal=formattedGrandTotal, grandTotalTT, cartItemCount, cartItemSum, message = "Xóa sản phẩm khỏi giỏ hàng thành công" });
        }

        public async Task<IActionResult> XoaHetGioHang()
        {
            int? userId = HttpContext.Session.GetInt32("id");
            // Tìm sản phẩm trong giỏ hàng
            var gioHang = _context.GioHangs.FirstOrDefault(g => g.Id == userId);


            string cartId = gioHang.MaGioHang;
            var cartItems = _context.ChiTietGioHangs.Where(c => c.MaGioHang == cartId);
            if (cartItems != null)
            {
                foreach (var cartItem in cartItems)
                {
                    // Xóa từng sản phẩm khỏi giỏ hàng
                    _context.ChiTietGioHangs.Remove(cartItem);
                }
                await _context.SaveChangesAsync();

            }
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }

        private string GenerateOrderCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var cartId = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return cartId;
        }

        private string GetShippingAddress(int idTK)
        {

            var thongTinCaNhan = _context.ThongTinCaNhans.FirstOrDefault(ttcn => ttcn.IdTk == idTK);
            if (thongTinCaNhan != null)
            {
                string diaChi = thongTinCaNhan.DiaChi;
                return diaChi;
            }
            else
            {
                return "Địa chỉ không khả dụng";
            }
        }

        public IActionResult Payment(int tongTien)
        {
            // Lưu thời gian bắt đầu thanh toán vào session
            HttpContext.Session.SetString("ThoiGianBatDau", DateTime.Now.ToString("o"));
            int? userId = HttpContext.Session.GetInt32("id");
            
            var taiKhoan = _context.TaiKhoans.Include(s => s.IdTkNavigation).FirstOrDefault(tk => tk.Id == userId);
            if (taiKhoan == null || string.IsNullOrEmpty(taiKhoan.IdTkNavigation.DiaChi))
            {
                // Display JavaScript alert
                TempData["ShowAlert"] = true;
                return RedirectToAction("ThongTinCaNhan", "Account");
            }
            // Thông số yêu cầu cần gửi tới hệ thống MoMo
            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";

            string partnerCode = "MOMOOJOI20210710";
            string accessKey = "iPXneGmrJH0G8FOP";
            string serectkey = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
            string orderInfo = "Thanh Toán Qua MoMo";
            string returnUrl = "http://localhost:5208/Cart/ReturnUrl";
            string notifyUrl = "http://localhost:5208/Cart/PaymentNotification";
			string amount = tongTien.ToString();
			string orderid = GenerateOrderCode(); // Mã đơn hàng
            string requestId = DateTime.Now.Ticks.ToString();
            string extraData = "";

            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyUrl + "&extraData=" +
                extraData;

            MoMoSecurity crypto = new MoMoSecurity();
            string signature = crypto.signSHA256(rawHash, serectkey);
            TempData["OrderId"] = orderid;
            // Build body json request
            JObject message = new()
            {
              { "partnerCode", partnerCode },
              { "accessKey", accessKey },
              { "requestId", requestId },
              { "amount",amount },
              { "orderId", orderid },
              { "orderInfo", orderInfo },
              { "returnUrl", returnUrl },
              { "notifyUrl", notifyUrl },
              { "extraData", extraData },
              { "requestType", "captureMoMoWallet" },
              { "signature", signature }
          };

            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());
            JObject jmessage = JObject.Parse(responseFromMomo);

            // Truy cập các trường trong JObject
            requestId = jmessage["requestId"]?.ToString();
            int errorCode = int.Parse(jmessage["errorCode"]?.ToString());
            string orderId = jmessage["orderId"]?.ToString();
            string localMessage = jmessage["localMessage"]?.ToString();
            HttpContext.Session.SetString("amount", amount);
            HttpContext.Session.SetString("orderid", orderid);



            return Redirect(jmessage.GetValue("payUrl").ToString());

        }
		
		public IActionResult ReturnUrl()
        {
            
           

            string amount = HttpContext.Session.GetString("amount");
            string orderid = HttpContext.Session.GetString("orderid");
			string discount =HttpContext.Session.GetString("voucherCode");
			string param = Request.QueryString.ToString().Substring(0, Request.QueryString.ToString().IndexOf("signature") - 1);
            param = WebUtility.UrlDecode(param);
           
            if (param.Contains("Bad request"))
            {
                int? userId = HttpContext.Session.GetInt32("id");

                var taiKhoan = _context.TaiKhoans.FirstOrDefault(tk => tk.Id == userId);
                string shippingAddress = "";
                if (taiKhoan != null)
                {
                    int idTK = taiKhoan.IdTk;
                    shippingAddress = GetShippingAddress(idTK);
                }

                if (int.TryParse(amount, out int parsedAmount))
                {
                    var orderCode = orderid;

                    // Tạo đơn hàng mới và lưu vào cơ sở dữ liệu với TrangThaiDH là "Thanh toán không thành công" do lỗi Bad request
                    var order = new DonHang
                    {
                        MaDh = orderCode,
                        NgayLap = DateTime.Now,
                        TongTien = parsedAmount,
                        DiaChiGiaoHang = shippingAddress,
						TrangThaiDh = "Thanh toán không thành công.",
						ThongTinDh = "Hủy"

					};

                    _context.DonHangs.Add(order);

                    var gioHang = _context.GioHangs.FirstOrDefault(g => g.Id == userId);
                    string maGioHang = gioHang != null ? gioHang.MaGioHang : null;
					var cartItems = _context.ChiTietGioHangs.Where(c => c.MaGioHang == maGioHang).ToList();
                    foreach (var item in cartItems)
                    {
                        var orderDetail = new ChiTietDonHang
                        {
                            MaDh = orderCode,
                            MaGioHang = maGioHang,
                            MaChiTiet = item.MaChiTiet,
                            MaSanPham = item.MaSanPham,
                            SoLuong = (int)item.SoLuong


                        };

                        _context.ChiTietDonHangs.Add(orderDetail);
                    }
                    try
                    {
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Đã có lỗi xảy ra khi lưu đơn hàng: " + ex.Message);
                    }
                }
                return RedirectToAction("ThanhToanThatBai", "SauKhiThanhToan");
            }
            else
            {
               
                // Thông tin khách hàng và địa chỉ giao hàng
                int? userId = HttpContext.Session.GetInt32("id");

                var taiKhoan = _context.TaiKhoans.FirstOrDefault(tk => tk.Id == userId);
                
                string shippingAddress = "";
                if (taiKhoan != null)
                {
                    int idTK = taiKhoan.IdTk;
                    shippingAddress = GetShippingAddress(idTK);
                }

                if (int.TryParse(amount, out int parsedAmount))
                {
                    var orderCode = orderid;

                    // Tạo đơn hàng mới và lưu vào cơ sở dữ liệu
                    var order = new DonHang
                    {
                        MaDh = orderCode,
                        NgayLap = DateTime.Now,
                        TongTien = parsedAmount,
                        DiaChiGiaoHang = shippingAddress,
						TrangThaiDh = "Đã thanh toán",
						ThongTinDh = "Đang Chuẩn Bị"

					};
					
					_context.DonHangs.Add(order);

                    var gioHang = _context.GioHangs.FirstOrDefault(g => g.Id == userId);
                    string maGioHang = gioHang != null ? gioHang.MaGioHang : null;
                    var cartItems = _context.ChiTietGioHangs.Where(c => c.MaGioHang == maGioHang).ToList();
                    foreach (var item in cartItems)
                    {
						var orderDetail = new ChiTietDonHang
						{
							MaDh = orderCode,
							MaGioHang = maGioHang,
							MaSanPham = item.MaSanPham,
                            MaChiTiet = item.MaChiTiet,
                            SoLuong = (int)item.SoLuong
                            

					};
						_context.ChiTietDonHangs.Add(orderDetail);

					}
                    
                    var gioHangCT = _context.GioHangs.FirstOrDefault(g => g.Id == userId);


                    string cartId = gioHang.MaGioHang;

                    var chiTiet = _context.ChiTietGioHangs.FirstOrDefault(g => g.MaGioHang == cartId);
                    int maChiTiet = chiTiet.MaChiTiet;
                    int retrievedSoLuong = (int)chiTiet.SoLuong;
                    


                    foreach (var item in cartItems)
                    {
                        var product = _context.ChiTietKichThuocs.FirstOrDefault(p => p.MaChiTiet == item.MaChiTiet);
                        if (product != null)
                        {
                            if (item.SoLuong!=0)
                            {
                                product.SoLuong -= item.SoLuong;
                            }
                            _context.ChiTietKichThuocs.Update(product);
                        }
                    }
					
					try
                    {
                        // Lưu đơn hàng vào cơ sở dữ liệu
                        _context.SaveChanges();
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Đã có lỗi xảy ra khi lưu đơn hàng: " + ex.Message);
                    }
                }



                // Gửi email xác nhận đơn hàng
                var thongTinCaNhan = _context.TaiKhoans.FirstOrDefault(t => t.Id == userId);
                string TenKhachHang = "";
                string Phone = "";
                string DiaChi = "";
                string Email = "";
                if (thongTinCaNhan != null)
                {
                    
                    TenKhachHang = thongTinCaNhan.IdTkNavigation.HoTen;
                    Phone = thongTinCaNhan.IdTkNavigation.Sdt.ToString();
                    DiaChi = thongTinCaNhan.IdTkNavigation.DiaChi;
                    Email = thongTinCaNhan.IdTkNavigation.Email;
                }



                
                var gioHang1 = _context.GioHangs.FirstOrDefault(g => g.Id == userId);
                string maGioHang1 = gioHang1 != null ? gioHang1.MaGioHang : null;
                string cartId1 = gioHang1.MaGioHang;
                var chiTiet1 = _context.ChiTietGioHangs.FirstOrDefault(g => g.MaGioHang == cartId1);
                int MaSP = chiTiet1.MaSanPham;
                var cartItems1 = _context.ChiTietGioHangs.Where(c => c.MaGioHang == cartId1).ToList();
                
                string NgayDat = DateTime.Now.ToString();
                var strSanPham = "";
                var thanhTien = decimal.Zero;
                var tongTien = decimal.Zero;
                foreach (var item in cartItems1)
                {
                    string tenSP = _context.SanPhams.FirstOrDefault(p => p.MaSanPham == item.MaSanPham)?.TenSanPham ?? "(Không Tìm Thấy Sản Phẩm)";
                    strSanPham+="<tr>";
                    strSanPham+="<td>"+tenSP + "</td>";
                    strSanPham += "<td>" + item.SoLuong + "</td>";
                    strSanPham += "<td>" + item.KichThuoc + "</td>";
                    strSanPham += "<td>" + item.Gia + "</td>";
                    strSanPham += "</tr>";
                    thanhTien += (decimal)(item.Gia * item.SoLuong);
                    tongTien += (decimal)parsedAmount;
                }
				//var giaTriGiamGia = _context..SingleOrDefault();
				string voucherCode = "";
				int giamGia = 0;

                if (thanhTien >= 1000000)
                {
                    voucherCode = GenerateRandomVoucherCode();
                    giamGia = 10;
                    

				}
                else if (thanhTien >= 500000)
                {
                    voucherCode = GenerateRandomVoucherCode();
                    giamGia = 5;
					
				}
                
				string orderCode1 = orderid;
				// Lưu thông tin mã giảm giá vào cơ sở dữ liệu
				if (!string.IsNullOrEmpty(voucherCode))
				{
                    // Tạo mới một bản ghi KhuyenMai để lưu thông tin mã giảm giá
                    var khuyenMai = new KhuyenMai
                    {
                        MaVoucher = voucherCode,
                        ThoiGianBatDau = DateTime.Now, // Ngày bắt đầu hiệu lực (có thể là ngày tạo đơn hàng)
                        ThoiGianKetThuc = DateTime.Now.AddDays(15), // Ngày kết thúc hiệu lực (ví dụ: 30 ngày sau ngày tạo đơn hàng)
                        PhanTramGiam = giamGia,
                        GhiChu = "Giảm Giá",
                        TenChuongTrinh = "Mua Giảm Giá",
                        SoLan = false,
                        Id = (int)userId,
						// Có thể có một trường id auto-generated bởi cơ sở dữ liệu
					};
                    
					// Thêm bản ghi vào cơ sở dữ liệu
					_context.KhuyenMais.Add(khuyenMai);
					_context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
				}
				var discountCode =  _context.KhuyenMais.FirstOrDefault(k => k.MaVoucher == discount);

				// kiềm tra mã giảm giá đang sài nếu thanh toán thành công thì chuyển sang SoLan = true
				if (discountCode != null)
				{
					// Cập nhật số lần sử dụng mã giảm giá
					discountCode.SoLan = true;
					_context.KhuyenMais.Update(discountCode);
					 _context.SaveChanges();

					
				}
				// ghi dữ liệu vào file .html (đường dẫn wwwroot/KDKTrangChu/TemplateCheckout/)
				string contentCustomer = System.IO.File.ReadAllText(Path.Combine("wwwroot/KDKTrangChu/TemplateCheckout/send2.html"));
				
				
				contentCustomer = contentCustomer.Replace("{{MaDonHang}}", orderCode1);
                contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham.ToString());
                contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", TenKhachHang.ToString());
                contentCustomer = contentCustomer.Replace("{{Phone}}", Phone.ToString());
                contentCustomer = contentCustomer.Replace("{{Email}}", Email.ToString());
                contentCustomer = contentCustomer.Replace("{{DiaChi}}", DiaChi.ToString());
                contentCustomer = contentCustomer.Replace("{{NgayDat}}", NgayDat.ToString());
                contentCustomer = contentCustomer.Replace("{{ThanhTien}}", thanhTien.ToString());
                contentCustomer = contentCustomer.Replace("{{TongTien}}", parsedAmount.ToString());
				// kiểm tra voucherCode tồn tại hay k nếu không thì xóa html
				if (!string.IsNullOrEmpty(voucherCode))
				{
					// 
					contentCustomer = contentCustomer.Replace("{{Voucher}}", voucherCode.ToString());
					contentCustomer = contentCustomer.Replace("{{ThoiGianBatDau}}", DateTime.Now.ToString()); 
					contentCustomer = contentCustomer.Replace("{{ThoiGianKetThuc}}", DateTime.Now.AddDays(15).ToString()); 
					
					contentCustomer = contentCustomer.Replace("{{GiamGia}}", giamGia.ToString()); 
				}
				else
				{
                    
                    // Nếu không có mã giảm giá, xóa hoàn toàn phần tử <table> khỏi nội dung email
                    contentCustomer = contentCustomer.Replace("<table cellspacing=\"1\" cellpadding=\"1\" border=\"1\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t   style=\"width:99%;vertical-align:top;margin-bottom:40px;padding:0\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t<tbody>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<tr>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<td valign=\"top\" width=\"50%\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tstyle=\"text-align:left;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif;border:0;padding:0\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<h2 style=\"color:#96588a;display:block;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif;font-size:18px;font-weight:bold;line-height:130%;margin:0 0 18px;text-align:left\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tMã Giảm Giá\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</h2>\r\n\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<address style=\"padding:12px;color:#636363;border:1px solid #e5e5e5\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<strong>{{Voucher}}</strong><br> Phần Trăm:<strong> {{GiamGia}}</strong><br><p>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\"{{ThoiGianBatDau}}\"\r\n\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</p> <br><p>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t{{ThoiGianKetThuc}}\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</address>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</tr>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t</tbody>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t</table>", "");
				}
				Task.Run(() => SendOrderConfirmationEmailAsync(Email, "Đơn Hàng:# " + orderCode1, "KDKMenShop gửi đến", contentCustomer.ToString()));




				//Admin
				string contentAdmin = System.IO.File.ReadAllText(Path.Combine("wwwroot/KDKTrangChu/TemplateCheckout/send1.html"));
                contentAdmin = contentAdmin.Replace("{{MaDonHang}}", orderCode1);
                contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham.ToString());
                contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", TenKhachHang.ToString());
                contentAdmin = contentAdmin.Replace("{{Phone}}", Phone.ToString());
                contentAdmin = contentAdmin.Replace("{{Email}}", Email.ToString());
                contentAdmin = contentAdmin.Replace("{{DiaChi}}", DiaChi.ToString());
				contentAdmin = contentAdmin.Replace("{{NgayDat}}", NgayDat.ToString());
				contentAdmin = contentAdmin.Replace("{{ThanhTien}}", thanhTien.ToString());
                contentAdmin = contentAdmin.Replace("{{TongTien}}", parsedAmount.ToString());
				Task.Run(() => SendOrderConfirmationEmailAsync(_configuration["Email:EmailAdmin"], "Đơn Hàng mới :# " + orderCode1, "Có Đơn Hàng Mới", contentAdmin.ToString()));
                foreach (var item in cartItems1)
                {
					_context.ChiTietGioHangs.Remove(item);
				}
                _context.SaveChanges();
                // Lấy thời gian bắt đầu từ session
                string startTimeString = HttpContext.Session.GetString("ThoiGianBatDau");
                DateTime startTime;


                // Tính toán thời gian thanh toán
                TrackPaymentTime();
                return RedirectToAction("ThanhToanThanhCong", "SauKhiThanhToan");

            }

        }

        //Phương thức này sẽ tính toán thời gian thanh toán và lưu kết quả vào Session
        private void TrackPaymentTime()
        {
            // Lấy thời gian bắt đầu từ Session
            string startTimeString = HttpContext.Session.GetString("ThoiGianBatDau");
            if (DateTime.TryParse(startTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime startTime))
            {
                DateTime endTime = DateTime.Now;
                TimeSpan totalTime = endTime - startTime;

                // Lưu tổng thời gian vào Session (giây)
                HttpContext.Session.SetInt32("TotalTime", (int)totalTime.TotalMilliseconds);
            }
            else
            {
                // Nếu không có thời gian bắt đầu, đặt mặc định là 0
                HttpContext.Session.SetInt32("TotalTime", 1);
            }
        }

        private string GenerateRandomVoucherCode()
		{
			var random = new Random();
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, 10)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}
		private async Task SendOrderConfirmationEmailAsync(string to, string orderCode, string subject, string body)
		{
			var fromAddress = new MailAddress("kikyoutnt33@gmail.com", "KDKShop");
			var toAddress = new MailAddress(to);
			const string fromPassword = "a m s j h a p u g c t b v s v a";

			var smtp = new SmtpClient
			{
				Host = "smtp.gmail.com",
				Port = 587,
				EnableSsl = true,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
			};

			using (var message = new MailMessage(fromAddress, toAddress)
			{
				Subject = subject,
				Body = body,
				IsBodyHtml = true // Đảm bảo hiển thị nội dung dưới dạng HTML
			})
			{
				await Task.Run(() => smtp.Send(message)); // Gửi email trong nền
			}
		}

	}
}
