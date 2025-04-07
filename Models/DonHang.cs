using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KDKMenShop.Models
{
	public partial class DonHang
	{
		public DonHang()
		{
			ChiTietDonHangs = new HashSet<ChiTietDonHang>();
		}
		[Required(ErrorMessage = "Vui lòng nhập mã đơn hàng.")]
		public string MaDh { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập ngày lập.")]
		public DateTime NgayLap { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập tổng tiền.")]
		public int TongTien { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
		public string DiaChiGiaoHang { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập trạng thái đơn hàng.")]
		public string TrangThaiDh { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập thông tin đơn hàng.")]
		public string ThongTinDh { get; set; }

		public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
	}
}