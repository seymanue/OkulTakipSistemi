namespace OkulTakipSistemi.Models
{
    public class Ogrenci
    {
        public int Id { get; set; }

        public string OgrenciNo { get; set; } = string.Empty;

        public string AdSoyad { get; set; } = string.Empty;

        public DateTime DogumTarihi { get; set; }

        public string Cinsiyet { get; set; } = string.Empty;

        public string Telefon { get; set; } = string.Empty;

        public DateTime KayitTarihi { get; set; }

        public string Adres { get; set; } = string.Empty;

        public bool Aktif { get; set; } = true;

        public int? SinifId { get; set; }

        public Sinif? Sinif { get; set; }
    }
}
