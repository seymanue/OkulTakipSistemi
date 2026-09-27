namespace OkulTakipSistemi.Models
{
    public class AylikBorc
    {
        public int Id { get; set; }

        public int OgrenciKaydiId { get; set; }

        public OgrenciKaydi? OgrenciKaydi { get; set; }

        public int Yil { get; set; }

        public int Ay { get; set; }

        public DateTime OdemeTarihi { get; set; }

        public decimal Tutar { get; set; }

        public decimal OdenenTutar { get; set; } = 0;

        public decimal KalanTutar
        {
            get
            {
                var kalan = Tutar - OdenenTutar;

                return kalan < 0 ? 0 : kalan;
            }
        }

        public bool Odendi
        {
            get
            {
                return KalanTutar <= 0;
            }
        }

        public string? Aciklama { get; set; }

        public ICollection<OdemeDagilimi> OdemeDagilimlari { get; set; }
            = new List<OdemeDagilimi>();
    }
}