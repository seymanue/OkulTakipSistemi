namespace OkulTakipSistemi.Models
{
    public class Sinif
    {
        public int Id { get; set; }

        public string Ad { get; set; } = string.Empty;

        public int OkulId { get; set; }

        public Okul? Okul { get; set; }

        public bool Aktif { get; set; } = true;
    }
}