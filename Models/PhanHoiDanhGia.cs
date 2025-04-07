using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class PhanHoiDanhGia
    {
        public int MaPhanHoi { get; set; }
        public string MaDanhGia { get; set; }
        public string PhanHoi { get; set; }

        public virtual DanhGiaSp MaDanhGiaNavigation { get; set; }
    }
}
