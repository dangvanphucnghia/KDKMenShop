namespace KDKMenShop.Models.Model
{
    public class CartItemModel
    {

        public int ProductId { get; set; }
        public string TenSP { get; set; }
        public int Soluong { get; set; }
        public int Gia { get; set; }
        public string KichThuoc { get; set; }
        public string image { get; set; }
        public decimal Total
        {
            get; set;

        }
        public int SoLuongTon { get; set; }

        public CartItemModel()
        {

        }
        public CartItemModel(SanPham sanpham, ChiTietKichThuoc chiTietKichThuoc)
        {
            ProductId = sanpham.MaSanPham;
            TenSP = sanpham.TenSanPham;
            Gia = sanpham.Gia;
            Soluong = 1;
            image = sanpham.HinhAnh;
            KichThuoc = chiTietKichThuoc.KichThuoc;
        }
    }
}
