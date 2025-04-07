using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class DanhGiaSp
    {
        public DanhGiaSp()
        {
            PhanHoiDanhGia = new HashSet<PhanHoiDanhGia>();
        }

        public string MaDanhGia { get; set; }
        public double DanhGia { get; set; }
        public string BinhLuan { get; set; }
        public string HinhAnh { get; set; }
        public int MaSanPham { get; set; }
        public int Id { get; set; }
        public string MaDh { get; set; }
        public DateTime NgayDanhGia { get; set; }

        public virtual TaiKhoan IdNavigation { get; set; }
        public virtual SanPham MaSanPhamNavigation { get; set; }
        public virtual ICollection<PhanHoiDanhGia> PhanHoiDanhGia { get; set; }
    }
}
