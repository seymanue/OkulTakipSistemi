namespace OkulTakipSistemi.Models
{
    public class OgrenciVeli
    {
        public int Id { get; set; }

        public int OgrenciId { get; set; }
        public Ogrenci? Ogrenci { get; set; }

        public int VeliId { get; set; }
        public Veli? Veli { get; set; }
    }
}
