using KDKMenShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KDKMenShop.Repository.Components
{
	public class sanphamloaiViewComponent : ViewComponent
	{
		private readonly ThoiTrangNamKDKContext _context;
		public sanphamloaiViewComponent(ThoiTrangNamKDKContext context)
		{
			_context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync() => View(await _context.LoaiSps.ToListAsync());
	}
}
