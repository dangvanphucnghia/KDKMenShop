using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class VatLieu
    {
        public VatLieu()
        {
            SanPhams = new HashSet<SanPham>();
        }

        public int MaVatLieu { get; set; }
        public string TenVatLieu { get; set; }

        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}
