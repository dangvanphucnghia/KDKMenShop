using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class ThongTinCaNhan
    {
        public ThongTinCaNhan()
        {
            TaiKhoans = new HashSet<TaiKhoan>();
        }

        public int IdTk { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string Sdt { get; set; }
        public string DiaChi { get; set; }
        public bool? IsEmailConfirmed { get; set; }
        public int? ConfirmationCode { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }

        public virtual ICollection<TaiKhoan> TaiKhoans { get; set; }
    }
}
