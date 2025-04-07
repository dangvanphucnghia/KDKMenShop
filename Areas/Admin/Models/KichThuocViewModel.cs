namespace KDKMenShop.Areas.Admin.Models
{
	public class KichThuocViewModel
	{
		public int MaChiTiet { get; set; }
		public string KichThuoc { get; set; }
		public int SoLuong { get; set; }
		public int MaSanPham { get; set; }
		public KichThuocViewModel()
		{
			DanhSachSanPham = new List<SanPhamViewModel>();
		}
		public List<SanPhamViewModel> DanhSachSanPham { get; set; }
	}
}
