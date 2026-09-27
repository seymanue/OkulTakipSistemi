namespace OkulTakipSistemi.Models
{
    public class UcretYonetimiViewModel
    {
        // Seçilen okul
        public int? SeciliOkulId { get; set; }

        // Okullar
        public List<Okul> Okullar { get; set; } = new();

        // Seçilen okulun ücret bilgisi
        public OkulUcreti? OkulUcreti { get; set; }
    }
}
