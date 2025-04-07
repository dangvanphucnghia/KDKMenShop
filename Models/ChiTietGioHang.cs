using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class ChiTietGioHang
    {
        public string MaGioHang { get; set; }
        public int MaSanPham { get; set; }
        public int Gia { get; set; }
        public int SoLuong { get; set; }
        public int TongTien { get; set; }
        public string KichThuoc { get; set; }
        public int MaChiTiet { get; set; }

        public virtual ChiTietKichThuoc Ma { get; set; }
        public virtual GioHang MaGioHangNavigation { get; set; }
        public virtual SanPham MaSanPhamNavigation { get; set; }
    }
}
