using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class PhanQuyenNhanVien
    {
        public PhanQuyenNhanVien()
        {
            ChucNangs = new HashSet<ChucNang>();
        }

        public int MaChucNang { get; set; }
        public int IdNhanVien { get; set; }
        public string GhiChu { get; set; }

        public virtual TaiKhoan IdNhanVienNavigation { get; set; }
        public virtual ICollection<ChucNang> ChucNangs { get; set; }
    }
}
