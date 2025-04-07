using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class ChiTietKichThuoc
    {
        public ChiTietKichThuoc()
        {
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
            ChiTietGioHangs = new HashSet<ChiTietGioHang>();
        }

        public int MaChiTiet { get; set; }
        public int MaSanPham { get; set; }
        public string KichThuoc { get; set; }
        public int SoLuong { get; set; }

        public virtual SanPham MaSanPhamNavigation { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
    }
}
