using System.ComponentModel.DataAnnotations;

namespace OkulTakipSistemi.Models
{
    public class OdemePlaniViewModel
    {
        public int OgrenciKaydiId { get; set; }

        public string OgrenciAdi { get; set; } = string.Empty;

        public string OkulAdi { get; set; } = string.Empty;

        public string SinifAdi { get; set; } = string.Empty;

        public decimal AylikUcret { get; set; }

        public decimal AylikOdenecekTutar { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int BaslangicYili { get; set; }

        [Required]
        [Range(1, 12)]
        public int BaslangicAyi { get; set; }

        [Required]
        [Range(1, 24)]
        public int TaksitSayisi { get; set; }

        public decimal ToplamTutar
        {
            get
            {
                return AylikOdenecekTutar * TaksitSayisi;
            }
        }
    }
}