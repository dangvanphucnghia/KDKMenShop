namespace KDKMenShop.Models.Model
{
    public class WishlistItemModel
    {

        public int ProductId { get; set; }
        public string TenSP { get; set; }
        public int Gia { get; set; }
        public string image { get; set; }
        public int GiaSale { get; set; }
        
        public WishlistItemModel()
        {

        }
        public WishlistItemModel(SanPham sanpham)
        {
            ProductId = sanpham.MaSanPham;
            TenSP = sanpham.TenSanPham;
            Gia = sanpham.Gia;
            image = sanpham.HinhAnh;
        }
    }
}
