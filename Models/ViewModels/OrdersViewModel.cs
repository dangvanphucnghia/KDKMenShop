using KDKMenShop.Areas.Admin.Models;

namespace KDKMenShop.Models.ViewModels
{
    public class OrdersViewModel
    {

		public string MaDH { get; set; }

		public DateTime NgayLap { get; set; }
		public int TongTien { get; set; }
		public string TrangThaiDH { get; set; }
        public string TenKhachHang { get; set; }
        public string TenSanPham { get; set; }
		public string KichThuoc { get; set; }
		public string ThongTinDH { get; set; }
		public string SoLuong { get; set; }
		public string MaSanPham { get; set; }
        public string DiaChiGiaoHang { get; set; }



        public int MaGioHang { get; set; }
        public int MaChiTiet { get; set; }
        public OrdersViewModel()
		{
            DanhSachSanPham = new List<SanPhamViewModel>();
        }
		public OrdersViewModel(DonHang donhang, SanPham sp, ChiTietDonHang ct, ChiTietKichThuoc chiTietKichThuoc)
		{
			MaDH = ct.MaDh;
			NgayLap = donhang.NgayLap;
			TrangThaiDH = donhang.TrangThaiDh;
			ThongTinDH = donhang.ThongTinDh;
			TenSanPham = sp.TenSanPham;
			KichThuoc = (chiTietKichThuoc.KichThuoc);
			SoLuong = ct.SoLuong.ToString();
			TongTien = donhang.TongTien;
			MaSanPham = sp.MaSanPham.ToString();
		}
        public List<SanPhamViewModel> DanhSachSanPham { get; set; }
        
           
        
    }
}
