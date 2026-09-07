namespace OkulTakipSistemi.Models
{
    public class UcretYonetimiViewModel
    {
        public List<OkulUcreti> Tarifeler { get; set; } = new();

        public List<Okul> Okullar { get; set; } = new();

        public List<UcretYonetimiOgrenciSatiri> Ogrenciler { get; set; } = new();

        public OkulUcreti YeniTarife { get; set; } = new()
        {
            OdemeSekli = "Taksitli",
            TaksitAySayisi = 12,
            BaslangicAyi = 9,
            GecerlilikTarihi = DateTime.Today
        };
    }

    public class UcretYonetimiOgrenciSatiri
    {
        public OgrenciKaydi Kayit { get; set; } = null!;

        public decimal? StandartVeliUcreti { get; set; }

        public OgrenciOzelUcret? OzelUcret { get; set; }

        public bool OdemePlaniVar { get; set; }

        public decimal? UygulanacakUcret =>
            OzelUcret?.OzelUcret ?? StandartVeliUcreti;
    }
}
