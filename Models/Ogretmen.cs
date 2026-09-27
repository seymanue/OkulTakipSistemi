namespace OkulTakipSistemi.Models
{
    public class Ogretmen
    {
        public int Id { get; set; }

        public string AdSoyad { get; set; } = string.Empty;


        public int KullaniciId { get; set; }

        public Kullanici? Kullanici { get; set; }
    }
}
