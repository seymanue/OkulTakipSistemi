namespace OkulTakipSistemi.Models
{
    public class OgrenciUcretDurumu
    {
        public int Id { get; set; }

        public int OgrenciKaydiId { get; set; }

        public OgrenciKaydi? OgrenciKaydi { get; set; }

        public bool Indirimli { get; set; }

        public bool Ucretsiz { get; set; }

        public decimal IndirimOrani { get; set; }

        public decimal? OzelFiyat { get; set; }

        public string? Aciklama { get; set; }
    }
}
