namespace OkulTakipSistemi.Models
{
    public class Yoklama
    {
        public int Id { get; set; }

        public int SinifId { get; set; }

        public Sinif? Sinif { get; set; }

        public int OgretmenId { get; set; }

        public Ogretmen? Ogretmen { get; set; }

        public DateTime Tarih { get; set; }

        public ICollection<YoklamaDetay> Detaylar { get; set; }
            = new List<YoklamaDetay>();
    }
}
