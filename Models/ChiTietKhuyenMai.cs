using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class ChiTietKhuyenMai
    {
        public ChiTietKhuyenMai()
        {
            SanPhams = new HashSet<SanPham>();
        }

        public int MaChiTietGiam { get; set; }
        public int MaSanPham { get; set; }
        public string MaVoucher { get; set; }
        public string Slug { get; set; }
        public string TenGiamGia { get; set; }

        public virtual KhuyenMai MaVoucherNavigation { get; set; }
        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}
