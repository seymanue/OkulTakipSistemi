namespace OkulTakipSistemi.Models
{
    public class OkulUcreti
    {
        public int Id { get; set; }

        public int OkulId { get; set; }

        public Okul? Okul { get; set; }

        public decimal ToplamUcret { get; set; }

        public decimal? KurumIcinBelirlenenUcret { get; set; }

        public decimal? StandartVeliUcreti { get; set; }

        public string OdemeSekli { get; set; } = "Taksitli";

        public int TaksitAySayisi { get; set; } = 12;

        public int BaslangicAyi { get; set; } = 9;

        public DateTime GecerlilikTarihi { get; set; }
public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public string? Aciklama { get; set; }

        public decimal AylikUcret
        {
            get
            {
                if (OdemeSekli == "Pesin")
                    return 0;

                if (TaksitAySayisi <= 0)
                    return 0;

                var veliUcreti = StandartVeliUcreti ?? ToplamUcret;

                return Math.Round(
                    veliUcreti / TaksitAySayisi,
                    2
                );
            }
        }

        public decimal UygulanacakStandartVeliUcreti =>
            StandartVeliUcreti ?? ToplamUcret;
    }
}
