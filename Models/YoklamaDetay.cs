namespace OkulTakipSistemi.Models
{
    public class YoklamaDetay
    {
        public int Id { get; set; }

        public int YoklamaId { get; set; }

        public Yoklama? Yoklama { get; set; }

        public int OgrenciId { get; set; }

        public Ogrenci? Ogrenci { get; set; }

        public bool Geldi { get; set; }
    }
}
