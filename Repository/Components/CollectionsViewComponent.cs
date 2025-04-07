using KDKMenShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KDKMenShop.Repository.Components
{
	public class CollectionsViewComponent : ViewComponent
	{
		private readonly ThoiTrangNamKDKContext _context;
		public CollectionsViewComponent(ThoiTrangNamKDKContext context)
		{
			_context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync() => View(await _context.BoSuuTaps.ToListAsync());
	}
}
