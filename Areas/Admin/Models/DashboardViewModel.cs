using KDKMenShop.Models.ViewModels;
using KDKMenShop.Models;
using System.ComponentModel.DataAnnotations;

namespace KDKMenShop.Areas.Admin.Models
{
	public class DashboardViewModel
    {
        public List<OrdersViewModel> DanhSachDonHang { get; set; }
        public List<SanPham> DanhSachSanPham { get; set; }
        public string[] Labels { get; set; }
        public int[] Data { get; set; }
        public string[] Labels1 { get; internal set; }
        public int[] Data1 { get; internal set; }
        public int[] Data2 { get; internal set; }
        public int[] Data3 { get; internal set; }
    }
}
