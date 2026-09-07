namespace OkulTakipSistemi.Models
{
    public class Kullanici
    {
        public int Id { get; set; }

        public string AdSoyad { get; set; } = string.Empty;

        public string KullaniciAdi { get; set; } = string.Empty;

        public string Sifre { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool EmailDogrulandi { get; set; } = false;

        public string? DogrulamaKodu { get; set; }

        public DateTime? DogrulamaKoduSonKullanma { get; set; }

        public int RolId { get; set; }

        public Rol? Rol { get; set; }
    }
}