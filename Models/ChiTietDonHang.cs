using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class ChiTietDonHang
    {
        public string MaDh { get; set; }
        public string MaGioHang { get; set; }
        public int MaSanPham { get; set; }
        public int MaChiTiet { get; set; }
        public int SoLuong { get; set; }

        public virtual ChiTietKichThuoc Ma { get; set; }
        public virtual DonHang MaDhNavigation { get; set; }
        public virtual GioHang MaGioHangNavigation { get; set; }
    }
}
