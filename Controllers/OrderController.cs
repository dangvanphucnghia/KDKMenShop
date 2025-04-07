using KDKMenShop.Models;
using KDKMenShop.Models.ViewModels;
using KDKMenShop.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Xml.Linq;

namespace KDKMenShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly ThoiTrangNamKDKContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public OrderController(ThoiTrangNamKDKContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            //Lấy ID người dùng từ session
            int? userId = HttpContext.Session.GetInt32("id");   
			if (userId == null || string.IsNullOrEmpty(HttpContext.Session.GetString("TaiKhoan")))
			{
				return RedirectToAction("DangNhap", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
			}
			var gioHang = _context.GioHangs
                               .Where(gh => gh.Id == userId)
							   .Select(gh => new { gh.MaGioHang })
                               .FirstOrDefault();
			
			//var maDH = _context.ChiTietDonHangs
			//					.Where(ct => ct.MaGioHang == maGioHang)
			//					.Select(ct => ct.MaDh).FirstOrDefault();
			// Lấy tên tài khoản từ session
			UpdateCartInfo();
			//int orderItemCount = _context.ChiTietDonHangs
			//			.Where(ct => ct.MaGioHang == maGioHang)
			//			.Select(g => g.MaDh).Distinct().Count(); ; // Assuming MaDh is numeric
			//ViewBag.OrderItemCount = orderItemCount;
			//HttpContext.Session.SetInt32("OrderItemCount", orderItemCount);
			//int orderSum = _context.ChiTietDonHangs
			//		  .Where(ct => ct.MaGioHang == maGioHang)
			//		  .Select(ct => ct.MaDh)
			//		  .Distinct()
			//		  .Join(_context.DonHangs,
			//				 maDh => maDh,
			//				dh => dh.MaDh,
			//				(maDh, dh) => dh.TongTien)
			//		  .Sum();
			//ViewBag.OrderSum = orderSum;
			//HttpContext.Session.SetInt32("OrderSum", orderSum);
			if (gioHang != null)
            {
                // Truy vấn để lấy MaDH từ MaGioHang
                // Lấy mã giỏ hàng của người dùng
                var maGioHang = gioHang.MaGioHang;
                //Chua test :D
                var maDHs = _context.ChiTietDonHangs
                                .Where(ct => ct.MaGioHang == maGioHang)
                                .Select(ct => ct.MaDh);
				var donHangs = _context.DonHangs
                                .Where(dh => maDHs.Contains(dh.MaDh))
                                .Select(dh => new OrdersViewModel
                                {
                                    MaDH = dh.MaDh,
                                    NgayLap = dh.NgayLap,
                                    TongTien = dh.TongTien,
                                    TrangThaiDH = dh.TrangThaiDh,
                                    TenSanPham = string.Join(", ", dh.ChiTietDonHangs.Where(ct => ct.MaDh == dh.MaDh)
														.SelectMany(ct => _context.SanPhams.Where(sp => sp.MaSanPham == ct.MaSanPham).Select(sp => sp.TenSanPham))
														.ToList()),
                                    SoLuong = string.Join(", ", dh.ChiTietDonHangs.Select(ct => ct.SoLuong)),
                                    MaSanPham = string.Join(", ", dh.ChiTietDonHangs.Select(ct => ct.MaSanPham)),
                                    KichThuoc = string.Join(", ", dh.ChiTietDonHangs
																	.Select(ct => _context.ChiTietKichThuocs
																	.Where(kt => kt.MaSanPham == ct.MaSanPham && kt.MaChiTiet == ct.MaChiTiet)
																	.Select(kt => kt.KichThuoc)
																	.FirstOrDefault()))
                                })
								.ToList();
                ViewBag.OrderItemCount = donHangs.Count;
                ViewBag.OrderSum = donHangs.Sum(o => o.TongTien);

                HttpContext.Session.SetInt32("OrderItemCount", donHangs.Count);
                HttpContext.Session.SetInt32("OrderSum", donHangs.Sum(o => (int)o.TongTien));

                return View(donHangs);

            }
            return View(new List<OrdersViewModel>());
            
        }
		// Hủy đơn hàng
        public async Task<IActionResult> DeleteOrder(string orderId)
        {
          
            int? userId = HttpContext.Session.GetInt32("id");
            var gioHang = await _context.GioHangs.FirstOrDefaultAsync(g => g.Id == userId);
            string cartId = gioHang.MaGioHang;

            var orderToDelete = await _context.DonHangs.Include(dh => dh.ChiTietDonHangs).FirstOrDefaultAsync(dh => dh.MaDh == orderId);

            if (orderToDelete != null)
            {
                // Xóa các chi tiết đơn hàng liên quan
                foreach (var item in orderToDelete.ChiTietDonHangs)
                {
                    var product = await _context.ChiTietKichThuocs.FirstOrDefaultAsync(p => p.MaChiTiet == item.MaChiTiet);
					if (product != null)
					{
						product.SoLuong += (int)item.SoLuong;
						_context.ChiTietKichThuocs.Update(product);
						await _context.SaveChangesAsync();
					}
				}
                var discount = await _context.KhuyenMais.FirstOrDefaultAsync(k => k.Id == userId);         // Kiểm tra xem discount có null không
				//tìm mã khuyến mãi gần nhất
                var latestVoucher = await _context.KhuyenMais
												.Where(v => v.MaVoucher == discount.MaVoucher)
												.OrderByDescending(v => v.ThoiGianBatDau)
												.FirstOrDefaultAsync();
                discount.SoLan = false;
                _context.Update(discount);
				var orderDetailsUpdate = await _context.ChiTietDonHangs.FirstOrDefaultAsync(ct => ct.MaDh == orderId && ct.MaGioHang == cartId);
				orderDetailsUpdate.MaDhNavigation.ThongTinDh = "Hủy";
				_context.Update(orderDetailsUpdate);
				await _context.SaveChangesAsync();
			}
			// Trả về kết quả xóa thành công
			return Json(new { success = true, message = "Xóa đơn hàng thành công" });
		}
        //public async Task<IActionResult> DeleteOrder(string orderId)
        //{
        //	int? userId = HttpContext.Session.GetInt32("id");
        //	var gioHang = _context.GioHangs.FirstOrDefault(g => g.Id == userId);
        //	string cartId = gioHang.MaGioHang;

        //	var orderToDelete = await _context.DonHangs.FirstOrDefaultAsync(dh => dh.MaDh == orderId);

        //	if (orderToDelete != null)
        //	{
        //		// Cập nhật trạng thái của đơn hàng
        //		orderToDelete.ThongTinDh = "Hủy";

        //		// Lưu thay đổi vào cơ sở dữ liệu
        //		await _context.SaveChangesAsync();
        //	}

        //	// Trả về kết quả xóa thành công
        //	return Json(new { success = true, message = "Chuyển đơn hàng sang trạng thái Hủy thành công" });
        //}
        private string GenerateMaDanhGia()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 10)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
		// xử lý đánh giá sản phẩm
		public IActionResult RateOrder(string orderId , string productId)
		{
            // truy vấn cơ sở dữ liệu để lấy thông tin đơn hàng
            

            int? userId = HttpContext.Session.GetInt32("id");
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            UpdateCartInfo();
            var order =  _context.DonHangs.FirstOrDefault(dh => dh.MaDh == orderId);
            // kiểm tra TaiKhoan đã đánh giá chưa
            bool hasRated = _context.DanhGiaSps.Any(d => d.MaDh == orderId && d.Id == userId);
            if (hasRated)
            {
                // rồi thì về trang đã đánh giá
                return RedirectToAction("AlreadyRatedPage");
            }
            List<int> MaSanPham = productId.Split(',').Select(int.Parse).ToList();

			// Kiểm tra xem đơn hàng có tồn tại hay không
			if (order == null)
			{
				// Nếu không tìm thấy đơn hàng, có thể chuyển hướng hoặc hiển thị thông báo lỗi
				return NotFound(); // Trả về trang 404 Not Found
			}
			
			var rateOrderViewModel = new RateOrderViewModel
			{
				// Gán các thuộc tính từ đối tượng đơn hàng vào đối tượng RateOrderViewModel
				// Lưu ý: Bạn cần thay thế các thuộc tính bên dưới bằng các thuộc tính tương ứng trong đối tượng DonHang
				
				Madh = order.MaDh,
				MaSanPham = MaSanPham,
				
				// Gán các thuộc tính khác tương ứng
			};
			// Trả về view với thông tin đơn hàng
			return View(rateOrderViewModel);
		}

		[HttpPost]
		public async Task<IActionResult> RateOrder(RateOrderViewModel model)
		{
			
			// Kiểm tra xem người dùng đã đăng nhập chưa
			int? userId = HttpContext.Session.GetInt32("id");
			if (userId == null)
			{
				return RedirectToAction("DangNhap", "Account");
			}

            // Kiểm tra xem người dùng đã đánh giá đơn hàng này trước đó hay không
            //bool hasRated = _context.DanhGiaSps.Any(d => d.MaDh == model.Madh && d.Id == userId);
            //if (hasRated)
            //{
            //    // Redirect to a page or display a message indicating that the user has already rated this order
            //    return RedirectToAction("AlreadyRatedPage");
            //}

            // Lặp qua danh sách mã sản phẩm và lưu đánh giá cho mỗi sản phẩm
            foreach (var productId in model.MaSanPham)
			{
               
                // Upload image if it exists
                if (model.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);

                    await model.ImageUpload.CopyToAsync(fs);

                    fs.Close();
                    model.HinhAnh = imageName;

                }
                DanhGiaSp danhGia = new DanhGiaSp
				{
					MaDanhGia = GenerateMaDanhGia(),
					DanhGia = model.DanhGia,
					BinhLuan = model.BinhLuan,
					NgayDanhGia = DateTime.Now,
					HinhAnh =  model.HinhAnh,
					MaSanPham = productId,
					MaDh = model.Madh,
					Id = userId.Value
				};
               
                // Lưu đánh giá vào cơ sở dữ liệu
                _context.DanhGiaSps.Add(danhGia);
			}
			await _context.SaveChangesAsync();

			// Cập nhật trạng thái đơn hàng (nếu cần)
			// Ví dụ: Đánh giá xong thì cập nhật trạng thái đơn hàng thành "Đã đánh giá"
			var orderToUpdate = await _context.DonHangs.FirstOrDefaultAsync(dh => dh.MaDh == model.Madh);
			if (orderToUpdate != null)
			{
				orderToUpdate.TrangThaiDh = "Đã đánh giá";
				_context.DonHangs.Update(orderToUpdate);
				await _context.SaveChangesAsync();
			}

			// Trả về kết quả cho người dùng (có thể làm redirect hoặc trả về thông báo)
			return RedirectToAction("RatingPage", "Order");
		}
		//trang cám ơn đã đánh giá
		public IActionResult RatingPage()
		{
			int? userId = HttpContext.Session.GetInt32("id");
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            UpdateCartInfo();
            return View();
		}
		//Trang đã tồn tại đánh giá
		public IActionResult AlreadyRatedPage()
		{
			int? userId = HttpContext.Session.GetInt32("id");
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            UpdateCartInfo();
			return View();
		}
		public void UpdateCartInfo()
		{
            int? userId = HttpContext.Session.GetInt32("id");
            var maGioHang = _context.GioHangs
							   .Where(gh => gh.Id == userId)
							   .Select(gh => gh.MaGioHang)
							   .FirstOrDefault();

			string taiKhoan = HttpContext.Session.GetString("TaiKhoan");
			ViewBag.cc = taiKhoan;

			int cartItemCount = (int)_context.ChiTietGioHangs
										.Where(g => g.MaGioHang == maGioHang)
										.Sum(g => g.SoLuong);

			ViewBag.CartItemCount = cartItemCount;
			HttpContext.Session.SetInt32("CartItemCount", cartItemCount);

			var wishlist = _context.Wishlists.FirstOrDefault(w => w.Id == userId);
			if (wishlist != null)
			{
				string wishlistID = wishlist.WishlistId;
				int wishlistItemCount = _context.ChiTietWishlists
												.Where(g => g.WishlistId == wishlistID)
												.Count();

				ViewBag.WishlistItemCount = wishlistItemCount;
				HttpContext.Session.SetInt32("WishlistItemCount", wishlistItemCount);
			}

			decimal cartItemSum = (decimal)_context.ChiTietGioHangs
												.Where(g => g.MaGioHang == maGioHang)
												.Sum(g => g.SoLuong * g.Gia);
			ViewBag.CartItemSum = cartItemSum;
			HttpContext.Session.SetInt32("CartItemSum", (int)cartItemSum);
			if (wishlist != null)
			{
				// Lấy số lượng các mục trong giỏ hàng từ cơ sở dữ liệu
				string wishlistID = wishlist.WishlistId;
				int wishlistItemCount = _context.ChiTietWishlists
									.Where(g => g.WishlistId == wishlistID).Count();

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
