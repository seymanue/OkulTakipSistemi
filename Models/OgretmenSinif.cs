namespace OkulTakipSistemi.Models
{
    public class OgretmenSinif
    {
        public int Id { get; set; }

        public int OgretmenId { get; set; }
        public Ogretmen? Ogretmen { get; set; }

        public int SinifId { get; set; }
        public Sinif? Sinif { get; set; }
    }
}
