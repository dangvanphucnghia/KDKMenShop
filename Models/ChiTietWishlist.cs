using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class ChiTietWishlist
    {
        public int MaSanPham { get; set; }
        public string WishlistId { get; set; }
        public string GhiChu { get; set; }

        public virtual SanPham MaSanPhamNavigation { get; set; }
        public virtual Wishlist Wishlist { get; set; }
    }
}
