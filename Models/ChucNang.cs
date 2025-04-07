using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class ChucNang
    {
        public int Id { get; set; }
        public string TenChucNang { get; set; }
        public int MaChucNang { get; set; }

        public virtual PhanQuyenNhanVien MaChucNangNavigation { get; set; }
    }
}
