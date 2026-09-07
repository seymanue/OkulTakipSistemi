namespace OkulTakipSistemi.Models
{
    public class OgrenciOzelUcret
    {
        public int Id { get; set; }

        public int OgrenciKaydiId { get; set; }

        public OgrenciKaydi? OgrenciKaydi { get; set; }

        public decimal OzelUcret { get; set; }

        public string? Aciklama { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public DateTime GuncellemeTarihi { get; set; } = DateTime.Now;
    }
}
