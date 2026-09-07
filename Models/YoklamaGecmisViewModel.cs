using System;
using System.Collections.Generic;

namespace OkulTakipSistemi.Models
{
    public class YoklamaGecmisViewModel
    {
        public int YoklamaId { get; set; }
        public string SinifAdi { get; set; }
        public DateTime Tarih { get; set; }

        public List<OgrenciYoklamaViewModel> Ogrenciler { get; set; }
    }

    public class OgrenciYoklamaViewModel
    {
        public string AdSoyad { get; set; }
        public string OgrenciNo { get; set; }
        public bool Geldi { get; set; }
    }
}