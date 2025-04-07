using System.ComponentModel.DataAnnotations;

namespace KDKMenShop.Areas.Admin.Models
{
	public class KhuyenMaiViewModel
	{
		[Required(ErrorMessage = "Tên chương trình không được để trống")]
		public string TenChuongTrinh { get; set; }

		[Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
		public DateTime ThoiGianBatDau { get; set; }

		[Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
		public DateTime ThoiGianKetThuc { get; set; }

		public string GhiChu { get; set; }
		

		[Required(ErrorMessage = "Vui lòng nhập phần trăm giảm giá")]
		[Range(1, 100, ErrorMessage = "Phần trăm giảm giá từ 1 đến 100")]
		public int PhanTramGiam { get; set; }

		public bool ChonTatCaSanPham { get; set; }

		public List<SanPhamViewModel> DanhSachSanPham { get; set; }
		public KhuyenMaiViewModel()
		{
			DanhSachSanPham = new List<SanPhamViewModel>();
		}
	}
}
