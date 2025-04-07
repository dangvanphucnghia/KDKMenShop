using KDKMenShop.Models.Model;

namespace KDKMenShop.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }
        public decimal GrandTotal {  get; set; }

        public List<ChiTietKichThuoc> ChiTietKichThuocs { get; set; }
    }
}
