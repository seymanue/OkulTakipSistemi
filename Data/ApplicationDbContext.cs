using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Okul> Okullar { get; set; }
        public DbSet<Sinif> Siniflar { get; set; }

        public DbSet<Ogrenci> Ogrenciler { get; set; }
        public DbSet<OgrenciKaydi> OgrenciKayitlari { get; set; }
        public DbSet<OgrenciUcretDurumu> OgrenciUcretDurumlari { get; set; }

        public DbSet<Veli> Veliler { get; set; }
        public DbSet<OgrenciVeli> OgrenciVeliler { get; set; }

        public DbSet<Ogretmen> Ogretmenler { get; set; }
        public DbSet<OgretmenSinif> OgretmenSiniflar { get; set; }

        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Rol> Roller { get; set; }

        public DbSet<Odeme> Odemeler { get; set; }
        public DbSet<OdemeDagilimi> OdemeDagilimlari { get; set; }

        public DbSet<OkulUcreti> OkulUcretleri { get; set; }
        public DbSet<OgrenciOzelUcret> OgrenciOzelUcretler { get; set; }

        public DbSet<Yoklama> Yoklamalar { get; set; }
        public DbSet<YoklamaDetay> YoklamaDetaylari { get; set; }

        public DbSet<AylikBorc> AylikBorclar { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Sinif>()
                .HasOne(x => x.Okul)
                .WithMany()
                .HasForeignKey(x => x.OkulId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OkulUcreti>()
                .HasOne(x => x.Okul)
                .WithMany()
                .HasForeignKey(x => x.OkulId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ogrenci>()
                .HasOne(x => x.Sinif)
                .WithMany()
                .HasForeignKey(x => x.SinifId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OgrenciKaydi>()
                .HasOne(x => x.Ogrenci)
                .WithMany()
                .HasForeignKey(x => x.OgrenciId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OgrenciKaydi>()
                .HasOne(x => x.Okul)
                .WithMany()
                .HasForeignKey(x => x.OkulId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OgrenciKaydi>()
                .HasOne(x => x.Sinif)
                .WithMany()
                .HasForeignKey(x => x.SinifId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ogretmen>()
                .HasOne(x => x.Kullanici)
                .WithMany()
                .HasForeignKey(x => x.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OgretmenSinif>()
                .HasOne(x => x.Ogretmen)
                .WithMany()
                .HasForeignKey(x => x.OgretmenId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OgretmenSinif>()
                .HasOne(x => x.Sinif)
                .WithMany()
                .HasForeignKey(x => x.SinifId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OgrenciVeli>()
                .HasOne(x => x.Ogrenci)
                .WithMany()
                .HasForeignKey(x => x.OgrenciId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OgrenciVeli>()
                .HasOne(x => x.Veli)
                .WithMany(x => x.OgrenciVeliler)
                .HasForeignKey(x => x.VeliId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Odeme>()
                .HasOne(x => x.Ogrenci)
                .WithMany()
                .HasForeignKey(x => x.OgrenciId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Odeme>()
                .HasOne(x => x.Kullanici)
                .WithMany()
                .HasForeignKey(x => x.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Kullanici>()
                .HasOne(x => x.Rol)
                .WithMany()
                .HasForeignKey(x => x.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Yoklama>()
                .HasOne(x => x.Sinif)
                .WithMany()
                .HasForeignKey(x => x.SinifId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Yoklama>()
                .HasOne(x => x.Ogretmen)
                .WithMany()
                .HasForeignKey(x => x.OgretmenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<YoklamaDetay>()
                .HasOne(x => x.Yoklama)
                .WithMany(x => x.Detaylar)
                .HasForeignKey(x => x.YoklamaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<YoklamaDetay>()
                .HasOne(x => x.Ogrenci)
                .WithMany()
                .HasForeignKey(x => x.OgrenciId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OgrenciUcretDurumu>()
                .HasOne(x => x.OgrenciKaydi)
                .WithMany()
                .HasForeignKey(x => x.OgrenciKaydiId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OgrenciOzelUcret>()
                .HasOne(x => x.OgrenciKaydi)
                .WithMany()
                .HasForeignKey(x => x.OgrenciKaydiId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OgrenciOzelUcret>()
                .HasIndex(x => x.OgrenciKaydiId)
                .IsUnique();

            modelBuilder.Entity<AylikBorc>()
                .HasOne(x => x.OgrenciKaydi)
                .WithMany()
                .HasForeignKey(x => x.OgrenciKaydiId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OdemeDagilimi>()
                .HasOne(x => x.Odeme)
                .WithMany(x => x.OdemeDagilimlari)
                .HasForeignKey(x => x.OdemeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OdemeDagilimi>()
                .HasOne(x => x.AylikBorc)
                .WithMany(x => x.OdemeDagilimlari)
                .HasForeignKey(x => x.AylikBorcId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
