using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KDKMenShop.Models
{
    public partial class ThoiTrangNamKDKContext : DbContext
    {
        public ThoiTrangNamKDKContext()
        {
        }

        public ThoiTrangNamKDKContext(DbContextOptions<ThoiTrangNamKDKContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BoSuuTap> BoSuuTaps { get; set; }
        public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }
        public virtual DbSet<ChiTietKhuyenMai> ChiTietKhuyenMais { get; set; }
        public virtual DbSet<ChiTietKichThuoc> ChiTietKichThuocs { get; set; }
        public virtual DbSet<ChiTietWishlist> ChiTietWishlists { get; set; }
        public virtual DbSet<ChucNang> ChucNangs { get; set; }
        public virtual DbSet<DanhGiaSp> DanhGiaSps { get; set; }
        public virtual DbSet<DonHang> DonHangs { get; set; }
        public virtual DbSet<GioHang> GioHangs { get; set; }
        public virtual DbSet<HinhAnhSp> HinhAnhSps { get; set; }
        public virtual DbSet<KhuyenMai> KhuyenMais { get; set; }
        public virtual DbSet<LoaiSp> LoaiSps { get; set; }
        public virtual DbSet<PhanHoiDanhGia> PhanHoiDanhGia { get; set; }
        public virtual DbSet<PhanQuyenNhanVien> PhanQuyenNhanViens { get; set; }
        public virtual DbSet<SanPham> SanPhams { get; set; }
        public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }
        public virtual DbSet<ThongTinCaNhan> ThongTinCaNhans { get; set; }
        public virtual DbSet<VatLieu> VatLieus { get; set; }
        public virtual DbSet<Wishlist> Wishlists { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=LAPTOP-VF44UALL;Database=ThoiTrangNamKDK;Trusted_Connection=True;TrustServerCertificate=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoSuuTap>(entity =>
            {
                entity.HasKey(e => e.MaBoSuuTap);

                entity.ToTable("BoSuuTap");

                entity.Property(e => e.MaBoSuuTap).ValueGeneratedNever();

                entity.Property(e => e.HinhBoSuuTap)
                    .HasMaxLength(255)
                    .IsFixedLength();

                entity.Property(e => e.Slug).HasMaxLength(50);

                entity.Property(e => e.TenBoSuuTap)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsFixedLength();
            });

            modelBuilder.Entity<ChiTietDonHang>(entity =>
            {
                entity.HasKey(e => new { e.MaDh, e.MaChiTiet })
                    .HasName("PK_ChiTietDonHang_1");

                entity.ToTable("ChiTietDonHang");

                entity.Property(e => e.MaDh)
                    .HasMaxLength(10)
                    .HasColumnName("MaDH")
                    .IsFixedLength();

                entity.Property(e => e.MaGioHang)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.HasOne(d => d.MaDhNavigation)
                    .WithMany(p => p.ChiTietDonHangs)
                    .HasForeignKey(d => d.MaDh)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChiTietDonHang_DonHang");

                entity.HasOne(d => d.MaGioHangNavigation)
                    .WithMany(p => p.ChiTietDonHangs)
                    .HasForeignKey(d => d.MaGioHang)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChiTietDonHang_GioHang");

                entity.HasOne(d => d.Ma)
                    .WithMany(p => p.ChiTietDonHangs)
                    .HasForeignKey(d => new { d.MaChiTiet, d.MaSanPham })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChiTietDonHang_ChiTietKichThuoc");
            });

            modelBuilder.Entity<ChiTietGioHang>(entity =>
            {
                entity.HasKey(e => new { e.MaGioHang, e.MaChiTiet });

                entity.ToTable("ChiTietGioHang");

                entity.Property(e => e.MaGioHang)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.KichThuoc)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.HasOne(d => d.MaGioHangNavigation)
                    .WithMany(p => p.ChiTietGioHangs)
                    .HasForeignKey(d => d.MaGioHang)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChiTietGioHang_GioHang");

                entity.HasOne(d => d.MaSanPhamNavigation)
                    .WithMany(p => p.ChiTietGioHangs)
                    .HasForeignKey(d => d.MaSanPham)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChiTietGioHang_SanPham");

                entity.HasOne(d => d.Ma)
                    .WithMany(p => p.ChiTietGioHangs)
                    .HasForeignKey(d => new { d.MaChiTiet, d.MaSanPham })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChiTietGioHang_ChiTietKichThuoc");
            });

            modelBuilder.Entity<ChiTietKhuyenMai>(entity =>
            {
                entity.HasKey(e => new { e.MaChiTietGiam, e.MaSanPham })
                    .HasName("PK_ChiTietKhuyenMai_1");

                entity.ToTable("ChiTietKhuyenMai");

                entity.Property(e => e.MaVoucher)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsFixedLength();

                entity.Property(e => e.Slug).HasMaxLength(50);

                entity.Property(e => e.TenGiamGia).HasMaxLength(50);

                entity.HasOne(d => d.MaVoucherNavigation)
                    .WithMany(p => p.ChiTietKhuyenMais)
                    .HasForeignKey(d => d.MaVoucher)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChiTietKhuyenMai_KhuyenMai");
            });

            modelBuilder.Entity<ChiTietKichThuoc>(entity =>
            {
                entity.HasKey(e => new { e.MaChiTiet, e.MaSanPham })
                    .HasName("PK_ChiTietKichThuoc_1");

                entity.ToTable("ChiTietKichThuoc");

                entity.Property(e => e.MaChiTiet).ValueGeneratedOnAdd();

                entity.Property(e => e.KichThuoc)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.HasOne(d => d.MaSanPhamNavigation)
                    .WithMany(p => p.ChiTietKichThuocs)
                    .HasForeignKey(d => d.MaSanPham)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChiTietKichThuoc_SanPham");
            });

            modelBuilder.Entity<ChiTietWishlist>(entity =>
            {
                entity.HasKey(e => new { e.MaSanPham, e.WishlistId });

                entity.ToTable("ChiTietWishlist");

                entity.Property(e => e.WishlistId)
                    .HasMaxLength(10)
                    .HasColumnName("WishlistID")
                    .IsFixedLength();

                entity.Property(e => e.GhiChu)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.HasOne(d => d.MaSanPhamNavigation)
                    .WithMany(p => p.ChiTietWishlists)
                    .HasForeignKey(d => d.MaSanPham)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChiTietWishlist_SanPham");

                entity.HasOne(d => d.Wishlist)
                    .WithMany(p => p.ChiTietWishlists)
                    .HasForeignKey(d => d.WishlistId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChiTietWishlist_Wishlist");
            });

            modelBuilder.Entity<ChucNang>(entity =>
            {
                entity.ToTable("ChucNang");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.Property(e => e.TenChucNang)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsFixedLength();

                entity.HasOne(d => d.MaChucNangNavigation)
                    .WithMany(p => p.ChucNangs)
                    .HasForeignKey(d => d.MaChucNang)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChucNang_PhanQuyenNhanVien");
            });

            modelBuilder.Entity<DanhGiaSp>(entity =>
            {
                entity.HasKey(e => e.MaDanhGia);

                entity.ToTable("DanhGiaSP");

                entity.Property(e => e.MaDanhGia)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.BinhLuan)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.HinhAnh)
                    .HasMaxLength(50)
                    .IsFixedLength();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.MaDh)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("MaDH")
                    .IsFixedLength();

                entity.Property(e => e.NgayDanhGia).HasColumnType("datetime");

                entity.HasOne(d => d.IdNavigation)
                    .WithMany(p => p.DanhGiaSps)
                    .HasForeignKey(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DanhGiaSP_TaiKhoan");

                entity.HasOne(d => d.MaSanPhamNavigation)
                    .WithMany(p => p.DanhGiaSps)
                    .HasForeignKey(d => d.MaSanPham)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DanhGiaSP_SanPham");
            });

            modelBuilder.Entity<DonHang>(entity =>
            {
                entity.HasKey(e => e.MaDh);

                entity.ToTable("DonHang");

                entity.Property(e => e.MaDh)
                    .HasMaxLength(10)
                    .HasColumnName("MaDH")
                    .IsFixedLength();

                entity.Property(e => e.DiaChiGiaoHang)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.NgayLap).HasColumnType("datetime");

                entity.Property(e => e.ThongTinDh)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("ThongTinDH");

                entity.Property(e => e.TrangThaiDh)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("TrangThaiDH");
            });

            modelBuilder.Entity<GioHang>(entity =>
            {
                entity.HasKey(e => e.MaGioHang);

                entity.ToTable("GioHang");

                entity.Property(e => e.MaGioHang)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Id).HasColumnName("id");
            });

            modelBuilder.Entity<HinhAnhSp>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.MaSanPham })
                    .HasName("PK_HinhAnhSP_1");

                entity.ToTable("HinhAnhSP");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("id");

                entity.Property(e => e.HinhAnh)
                    .HasMaxLength(100)
                    .IsFixedLength();

                entity.HasOne(d => d.MaSanPhamNavigation)
                    .WithMany(p => p.HinhAnhSps)
                    .HasForeignKey(d => d.MaSanPham)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HinhAnhSP_SanPham");
            });

            modelBuilder.Entity<KhuyenMai>(entity =>
            {
                entity.HasKey(e => e.MaVoucher);

                entity.ToTable("KhuyenMai");

                entity.Property(e => e.MaVoucher)
                    .HasMaxLength(50)
                    .IsFixedLength();

                entity.Property(e => e.GhiChu)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.TenChuongTrinh)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ThoiGianBatDau).HasColumnType("datetime");

                entity.Property(e => e.ThoiGianKetThuc).HasColumnType("datetime");

                entity.HasOne(d => d.IdNavigation)
                    .WithMany(p => p.KhuyenMais)
                    .HasForeignKey(d => d.Id)
                    .HasConstraintName("FK_KhuyenMai_TaiKhoan");
            });

            modelBuilder.Entity<LoaiSp>(entity =>
            {
                entity.HasKey(e => e.MaLoaiSp);

                entity.ToTable("LoaiSP");

                entity.Property(e => e.MaLoaiSp).HasColumnName("MaLoaiSP");

                entity.Property(e => e.Slug).HasMaxLength(50);

                entity.Property(e => e.TenLoaiSp)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("TenLoaiSP");
            });

            modelBuilder.Entity<PhanHoiDanhGia>(entity =>
            {
                entity.HasKey(e => e.MaPhanHoi)
                    .HasName("PK_PhanHoiDanhGia_1");

                entity.Property(e => e.MaDanhGia)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.PhanHoi)
                    .IsRequired()
                    .HasMaxLength(55);

                entity.HasOne(d => d.MaDanhGiaNavigation)
                    .WithMany(p => p.PhanHoiDanhGia)
                    .HasForeignKey(d => d.MaDanhGia)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PhanHoiDanhGia_DanhGiaSP");
            });

            modelBuilder.Entity<PhanQuyenNhanVien>(entity =>
            {
                entity.HasKey(e => e.MaChucNang)
                    .HasName("PK_PhanQuyenNhanVien_1");

                entity.ToTable("PhanQuyenNhanVien");

                entity.Property(e => e.MaChucNang).ValueGeneratedNever();

                entity.Property(e => e.GhiChu)
                    .HasMaxLength(255)
                    .IsFixedLength();

                entity.Property(e => e.IdNhanVien).HasColumnName("idNhanVien");

                entity.HasOne(d => d.IdNhanVienNavigation)
                    .WithMany(p => p.PhanQuyenNhanViens)
                    .HasForeignKey(d => d.IdNhanVien)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PhanQuyenNhanVien_TaiKhoan");
            });

            modelBuilder.Entity<SanPham>(entity =>
            {
                entity.HasKey(e => e.MaSanPham);

                entity.ToTable("SanPham");

                entity.Property(e => e.MaSanPham).ValueGeneratedOnAdd();

                entity.Property(e => e.HinhAnh)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.MaLoaiSp).HasColumnName("MaLoaiSP");

                entity.Property(e => e.MoTa)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.TenSanPham)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.MaBoSuuTapNavigation)
                    .WithMany(p => p.SanPhams)
                    .HasForeignKey(d => d.MaBoSuuTap)
                    .HasConstraintName("FK_SanPham_BoSuuTap");

                entity.HasOne(d => d.MaLoaiSpNavigation)
                    .WithMany(p => p.SanPhams)
                    .HasForeignKey(d => d.MaLoaiSp)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SanPham_LoaiSP");

                entity.HasOne(d => d.MaVatLieuNavigation)
                    .WithMany(p => p.SanPhams)
                    .HasForeignKey(d => d.MaVatLieu)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SanPham_VatLieu");

                entity.HasOne(d => d.Ma)
                    .WithMany(p => p.SanPhams)
                    .HasForeignKey(d => new { d.MaChiTietGiam, d.MaSanPham })
                    .HasConstraintName("FK_SanPham_ChiTietKhuyenMai");
            });

            modelBuilder.Entity<TaiKhoan>(entity =>
            {
                entity.ToTable("TaiKhoan");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.IdTk).HasColumnName("idTK");

                entity.Property(e => e.LoaiTk)
                    .IsRequired()
                    .HasMaxLength(55)
                    .HasColumnName("LoaiTK")
                    .IsFixedLength();

                entity.Property(e => e.MatKhau)
                    .IsRequired()
                    .HasMaxLength(55)
                    .IsFixedLength();

                entity.Property(e => e.TenDangNhap)
                    .IsRequired()
                    .HasMaxLength(55)
                    .IsFixedLength();

                entity.HasOne(d => d.IdTkNavigation)
                    .WithMany(p => p.TaiKhoans)
                    .HasForeignKey(d => d.IdTk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TaiKhoan_ThongTinCaNhan");
            });

            modelBuilder.Entity<ThongTinCaNhan>(entity =>
            {
                entity.HasKey(e => e.IdTk);

                entity.ToTable("ThongTinCaNhan");

                entity.Property(e => e.IdTk).HasColumnName("idTK");

                entity.Property(e => e.DiaChi).HasMaxLength(250);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.HoTen)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.NgayBatDau).HasColumnType("datetime");

                entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");

                entity.Property(e => e.Sdt).HasMaxLength(11);

                entity.Property(e => e.TrangThai)
                    .IsRequired()
                    .HasMaxLength(10);
            });

            modelBuilder.Entity<VatLieu>(entity =>
            {
                entity.HasKey(e => e.MaVatLieu);

                entity.ToTable("VatLieu");

                entity.Property(e => e.MaVatLieu).ValueGeneratedNever();

                entity.Property(e => e.TenVatLieu)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.HasKey(e => e.WishlistId);

                entity.ToTable("Wishlist");

                entity.Property(e => e.WishlistId)
                    .HasMaxLength(10)
                    .HasColumnName("WishlistID")
                    .IsFixedLength();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.HasOne(d => d.IdNavigation)
                    .WithMany(p => p.Wishlists)
                    .HasForeignKey(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Wishlist_TaiKhoan");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
