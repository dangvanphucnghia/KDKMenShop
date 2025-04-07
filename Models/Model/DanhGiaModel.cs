namespace KDKMenShop.Models.Model
{
    public class DanhGiaModel
    {
        public string MaDanhGia { get; set; }
        public double DanhGia { get; set; }
        public string BinhLuan { get; set; }
        public string HinhAnh { get; set; }
        public int MaSanPham { get; set; }
        public string KichThuoc { get; set; }
        public int SoLuong {  get; set; }   
        public int Id { get; set; }
        public string MaDh { get; set; }
        public string TenKhachHang { get; set; }
        public string PhanHoi { get; set; }
        public int LuotDanhGia { get; set; }
		public int LuotMua { get; set; }
        public DateTime NgayDanhGia { get;set; }
	}
}
