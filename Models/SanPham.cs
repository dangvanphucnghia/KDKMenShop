using KDKMenShop.Repository.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace KDKMenShop.Models
{
	public partial class SanPham
	{
		public SanPham()
		{
			ChiTietGioHangs = new HashSet<ChiTietGioHang>();
			ChiTietKichThuocs = new HashSet<ChiTietKichThuoc>();
			ChiTietWishlists = new HashSet<ChiTietWishlist>();
			DanhGiaSps = new HashSet<DanhGiaSp>();
			HinhAnhSps = new HashSet<HinhAnhSp>();
		}

		public int MaSanPham { get; set; }
		public string TenSanPham { get; set; }
		public int MaVatLieu { get; set; }
		public int Gia { get; set; }
		public string HinhAnh { get; set; }
		public string MoTa { get; set; }
		public int MaLoaiSp { get; set; }
		public int? MaBoSuuTap { get; set; }
		public int? MaChiTietGiam { get; set; }
		[NotMapped]
		[FileExtension]
		public IFormFile ImageUpload { get; set; }
		public virtual VatLieu MaVatLieuNavigation { get; set; }
		public virtual ChiTietKhuyenMai Ma { get; set; }
		public virtual BoSuuTap MaBoSuuTapNavigation { get; set; }
		public virtual LoaiSp MaLoaiSpNavigation { get; set; }
		public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
		public virtual ICollection<ChiTietKichThuoc> ChiTietKichThuocs { get; set; }
		public virtual ICollection<ChiTietWishlist> ChiTietWishlists { get; set; }
		public virtual ICollection<DanhGiaSp> DanhGiaSps { get; set; }
		public virtual ICollection<HinhAnhSp> HinhAnhSps { get; set; }
	}
}