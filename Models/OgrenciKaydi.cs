namespace OkulTakipSistemi.Models
{
    public class OgrenciKaydi
    {
        public int Id { get; set; }

        public int OgrenciId { get; set; }
        public Ogrenci? Ogrenci { get; set; }

        public int OkulId { get; set; }
        public Okul? Okul { get; set; }

        public int SinifId { get; set; }
        public Sinif? Sinif { get; set; }

        public bool Aktif { get; set; } = true;
    }
}
