using System;
using System.Collections.Generic;

namespace KDKMenShop.Models
{
    public partial class Wishlist
    {
        public Wishlist()
        {
            ChiTietWishlists = new HashSet<ChiTietWishlist>();
        }

        public string WishlistId { get; set; }
        public int Id { get; set; }

        public virtual TaiKhoan IdNavigation { get; set; }
        public virtual ICollection<ChiTietWishlist> ChiTietWishlists { get; set; }
    }
}
