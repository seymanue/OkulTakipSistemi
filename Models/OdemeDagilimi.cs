namespace OkulTakipSistemi.Models
{
    public class OdemeDagilimi
    {
        public int Id { get; set; }

        public int OdemeId { get; set; }
        public Odeme? Odeme { get; set; }

        public int AylikBorcId { get; set; }
        public AylikBorc? AylikBorc { get; set; }

        public decimal Tutar { get; set; }
    }
}
