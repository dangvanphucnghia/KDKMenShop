using KDKMenShop.Models.Model;

namespace KDKMenShop.Models.ViewModels
{
    public class ProductViewModel
    {
        public SanPham Product { get; set; }
        public List<HinhAnhSp> Images { get; set; }
        public List<ChiTietKichThuoc> ChiTietKichThuocs { get; set; }
        public List<DanhGiaModel> DanhGiaSps { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
    }

}
