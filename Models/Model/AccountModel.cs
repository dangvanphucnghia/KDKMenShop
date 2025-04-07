using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace KDKMenShop.Models.Model
{
    public class AccountModel
    {
        [Key]
        public int IdTK { get; set; } // Đặt tên thuộc tính là Id thay vì idTK 
        public string loaiTK { get; set; }
        public string TaiKhoan { get; set; }
        public string Password { get; set; }
        public string confirmPassword { get; set; }
        public int confirmationCode { get; set; }
        public string Name { get; set; }
        [EmailAddress(ErrorMessage = "Định dạng email không đúng.")]
        public string Email { get; set; }
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại phải có từ 10 đến 11 chữ số.")]
        public string SDT { get; set; }
        public string Diachi { get; set; }
    }
}
