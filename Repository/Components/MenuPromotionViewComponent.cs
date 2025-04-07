using KDKMenShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KDKMenShop.Repository.Components
{
    public class MenuPromotionViewComponent : ViewComponent
    {
        private readonly ThoiTrangNamKDKContext _context;
        public MenuPromotionViewComponent(ThoiTrangNamKDKContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync() => View(await _context.ChiTietKhuyenMais.Include(s=>s.MaVoucherNavigation).ToListAsync());

		
	}
}
