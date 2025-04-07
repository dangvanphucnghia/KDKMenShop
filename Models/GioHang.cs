using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class GioHang
    {
        public GioHang()
        {
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
            ChiTietGioHangs = new HashSet<ChiTietGioHang>();
        }

        public string MaGioHang { get; set; }
        public int Id { get; set; }

        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
    }
}
