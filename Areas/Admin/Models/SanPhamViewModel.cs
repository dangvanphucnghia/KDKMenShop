namespace KDKMenShop.Areas.Admin.Models
{
	public class SanPhamViewModel
	{
		//Giảm giá
		public int MaSanPham { get; set; }
		public int? MaChiTietGiam { get; set; }
		public string TenSanPham { get; set; }
		public string Slug { get; set; }
		public string TenGiamGia { get; set; }
		public string HinhAnh { get; set; }
        public bool Selected { get; set; }




		//đơn hàng
        public int MaChiTiet { get; set; }
        public string KichThuoc { get; set; }
        public int SoLuong { get; set; }
    }
}
