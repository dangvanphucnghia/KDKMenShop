using KDKMenShop.Repository.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace KDKMenShop.Models.ViewModels
{
    public class RateOrderViewModel
    {
        public int MaDanhGia { get; set; }
        public int DanhGia { get; set; }
        public string BinhLuan { get; set; }
        public string HinhAnh { get; set; }
        public List<int> MaSanPham { get; set; }
        public string Madh { get; set; }
        public int id { get; set; }
        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpload { get; set; }

    }
}
