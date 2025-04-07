using KDKMenShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KDKMenShop.Repository.Components
{
    public class HeroCollectionViewComponent : ViewComponent
    {
        private readonly ThoiTrangNamKDKContext _context;
        public HeroCollectionViewComponent(ThoiTrangNamKDKContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync() => View(await _context.BoSuuTaps.ToListAsync());

		
	}
}
