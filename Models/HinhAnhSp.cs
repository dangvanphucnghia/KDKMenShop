using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class HinhAnhSp
    {
        public int Id { get; set; }
        public int MaSanPham { get; set; }
        public string HinhAnh { get; set; }

        public virtual SanPham MaSanPhamNavigation { get; set; }
    }
}
