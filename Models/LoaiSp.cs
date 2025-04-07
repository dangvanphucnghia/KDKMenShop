using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;
namespace KDKMenShop.Models
{
	public partial class LoaiSp
	{
		public LoaiSp()
		{
			SanPhams = new HashSet<SanPham>();
		}

		public int MaLoaiSp { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập tên loại sản phẩm.")]
		public string TenLoaiSp { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập SLug loại sản phẩm.")]
		public string Slug { get; set; }

		public virtual ICollection<SanPham> SanPhams { get; set; }
	}
}