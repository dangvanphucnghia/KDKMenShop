using KDKMenShop.Repository.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace KDKMenShop.Models
{
	public partial class BoSuuTap
	{
		public BoSuuTap()
		{
			SanPhams = new HashSet<SanPham>();
		}

		public int MaBoSuuTap { get; set; }
		public string TenBoSuuTap { get; set; }
		public string HinhBoSuuTap { get; set; }
		public string Slug { get; set; }
		[NotMapped]
		[FileExtension]
		public IFormFile ImageUpload { get; set; }

		public virtual ICollection<SanPham> SanPhams { get; set; }
	}
}