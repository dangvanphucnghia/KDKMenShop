using KDKMenShop.Models;
using KDKMenShop.Models.ProductModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KDKMenShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        ThoiTrangNamKDKContext _context = new ThoiTrangNamKDKContext();
		private static bool KiemTraKhuyenMai(KhuyenMai khuyenMai)
		{
			DateTime now = DateTime.Now;
			return now >= khuyenMai.ThoiGianBatDau && now <= khuyenMai.ThoiGianKetThuc;
		}
		[HttpGet("{all}")]
        public IEnumerable<Product> GetALLProducts()
        {

            var productsWithDiscount = (from p in _context.SanPhams.AsNoTracking()
                                        join ct in _context.ChiTietKhuyenMais.AsNoTracking() on p.MaSanPham equals ct.MaSanPham into ctGroup
                                        from ct in ctGroup.DefaultIfEmpty()
                                        join km in _context.KhuyenMais.AsNoTracking() on ct.MaVoucher equals km.MaVoucher into kmGroup
                                        from km in kmGroup.DefaultIfEmpty()
                                        select new
                                        {
                                            SanPham = p,
                                            KhuyenMai = km
                                        }).ToList();


            var productList = new List<Product>();

            foreach (var item in productsWithDiscount)
            {
                int originalPrice = item.SanPham.Gia; // Giá gốc của sản phẩm
                int salePrice = originalPrice; // Giá sale, mặc định là giá gốc
                int PhanTram = 0;

                if (item.KhuyenMai != null && KiemTraKhuyenMai(item.KhuyenMai))
				{
					// Nếu có khuyến mãi và đang trong thời gian khuyến mãi, tính giá sau khi giảm giá
					originalPrice -= (originalPrice * item.KhuyenMai.PhanTramGiam) / 100;
                     PhanTram = item.KhuyenMai.PhanTramGiam;
                }

                // Thêm sản phẩm vào danh sách sản phẩm với giá đã tính toán
                productList.Add(new Product
                {
                    MaSanPham = item.SanPham.MaSanPham,
                    MaLoaiSp = item.SanPham.MaLoaiSp,
                    TenSanPham = item.SanPham.TenSanPham,
                    HinhAnh = item.SanPham.HinhAnh,
                    Gia = salePrice,
                    GiaSale = originalPrice,

                    PhanTram = PhanTram
                }) ;
            }

            return productList;
        }

       
        [HttpGet("category/{maloai}")]
        public IEnumerable<Product> GetProductsByCategory(int maloai)
        {
            return (from p in _context.SanPhams.AsNoTracking()
                    where p.MaLoaiSp == maloai
                    join ct in _context.ChiTietKhuyenMais.AsNoTracking() on p.MaSanPham equals ct.MaSanPham into ctGroup
                    from ct in ctGroup.DefaultIfEmpty()
                    join km in _context.KhuyenMais.AsNoTracking() on ct.MaVoucher equals km.MaVoucher into kmGroup
                    from km in kmGroup.DefaultIfEmpty()
                    let PhanTram = (km != null && KiemTraKhuyenMai(km)) ? km.PhanTramGiam : 0
                    let salePrice = (PhanTram > 0) ? p.Gia - (p.Gia * PhanTram / 100) : p.Gia
                    select new Product
                    {
                        MaSanPham = p.MaSanPham,
                        MaLoaiSp = p.MaLoaiSp,
                        TenSanPham = p.TenSanPham,
                        HinhAnh = p.HinhAnh,
                        Gia = p.Gia,
                        GiaSale = salePrice,
                        PhanTram = PhanTram
                    }).ToList();
        }

        [HttpGet("collection/{mabosuutap}")]
                                   
        public IEnumerable<Product> GetProductsByCollection(int mabosuutap)
        {
            var SanPham = (from p in _context.SanPhams
                           where p.MaBoSuuTap == mabosuutap
                           join ct in _context.ChiTietKhuyenMais on p.MaSanPham equals ct. MaSanPham into ctGroup
                           from ct in ctGroup.DefaultIfEmpty()
                           join km in _context.KhuyenMais on ct.MaVoucher equals km.MaVoucher into kmGroup
                           from km in kmGroup.DefaultIfEmpty()
                           select new
                           {
                               SanPham = p,
                               KhuyenMai = km
                           }).ToList();

            var productList = new List<Product>();
            foreach (var item in SanPham)
            {

                int originalPrice = item.SanPham.Gia; // Giá gốc của sản phẩm
                int salePrice = originalPrice; // Giá sale, mặc định là giá gốc
                if (item.KhuyenMai != null)
                {
                    // Nếu có khuyến mãi, tính giá sau khi giảm giá
                    salePrice -= (item.KhuyenMai.PhanTramGiam * originalPrice) / 100;
                }

                // Thêm sản phẩm vào danh sách sản phẩm với giá đã tính toán
                productList.Add(new Product
                {
                    MaSanPham = item.SanPham.MaSanPham,
                    MaLoaiSp = item.SanPham.MaLoaiSp,
                    TenSanPham = item.SanPham.TenSanPham,
                    HinhAnh = item.SanPham.HinhAnh,
                    Gia = originalPrice,
                    GiaSale = salePrice,
                });
            }

            return productList;
        }
    }
}
