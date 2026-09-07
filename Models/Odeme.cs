namespace OkulTakipSistemi.Models
{
    public class Odeme
    {
        public int Id { get; set; }

        public int OgrenciId { get; set; }

        public Ogrenci? Ogrenci { get; set; }

        public decimal Tutar { get; set; }

        public DateTime OdemeTarihi { get; set; }

        public string? Aciklama { get; set; }

        public string Durum { get; set; } = "Ödendi";

        // Ödemeyi sisteme giren kullanıcı
        public int KullaniciId { get; set; }

        public Kullanici? Kullanici { get; set; }

        public ICollection<OdemeDagilimi> OdemeDagilimlari { get; set; }
            = new List<OdemeDagilimi>();
    }
}