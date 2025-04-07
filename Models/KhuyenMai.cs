using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class KhuyenMai
    {
        public KhuyenMai()
        {
            ChiTietKhuyenMais = new HashSet<ChiTietKhuyenMai>();
        }

        public string MaVoucher { get; set; }
        public string TenChuongTrinh { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string GhiChu { get; set; }
        public int PhanTramGiam { get; set; }
        public bool? SoLan { get; set; }
        public int? Id { get; set; }

        public virtual TaiKhoan IdNavigation { get; set; }
        public virtual ICollection<ChiTietKhuyenMai> ChiTietKhuyenMais { get; set; }
    }
}
