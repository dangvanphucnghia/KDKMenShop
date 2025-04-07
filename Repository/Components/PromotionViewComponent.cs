using KDKMenShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KDKMenShop.Repository.Components
{
    public class PromotionViewComponent : ViewComponent
    {
        private readonly ThoiTrangNamKDKContext _context;
        public PromotionViewComponent(ThoiTrangNamKDKContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync() => View(await _context.ChiTietKhuyenMais.Include(s => s.SanPhams)
				.ThenInclude(sp => sp.MaLoaiSpNavigation).Include(s=>s.MaVoucherNavigation).ToListAsync());

		
	}
}
