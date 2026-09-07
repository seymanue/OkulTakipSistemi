namespace OkulTakipSistemi.Models
{
    public class Veli
    {
        public int Id { get; set; }

        public string AdSoyad { get; set; } = string.Empty;

        public string Telefon { get; set; } = string.Empty;

        public string Yakinlik { get; set; } = string.Empty;

        public ICollection<OgrenciVeli> OgrenciVeliler { get; set; }
            = new List<OgrenciVeli>();
    }
}
