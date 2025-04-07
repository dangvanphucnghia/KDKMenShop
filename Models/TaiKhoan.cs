using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class TaiKhoan
    {
        public TaiKhoan()
        {
            DanhGiaSps = new HashSet<DanhGiaSp>();
            KhuyenMais = new HashSet<KhuyenMai>();
            PhanQuyenNhanViens = new HashSet<PhanQuyenNhanVien>();
            Wishlists = new HashSet<Wishlist>();
        }

        public int Id { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string LoaiTk { get; set; }
        public int IdTk { get; set; }

        public virtual ThongTinCaNhan IdTkNavigation { get; set; }
        public virtual ICollection<DanhGiaSp> DanhGiaSps { get; set; }
        public virtual ICollection<KhuyenMai> KhuyenMais { get; set; }
        public virtual ICollection<PhanQuyenNhanVien> PhanQuyenNhanViens { get; set; }
        public virtual ICollection<Wishlist> Wishlists { get; set; }
    }
}
