using System;
using System.Collections.Generic;

namespace OkulTakipSistemi.Models
{
    public class OgrenciKayitFormViewModel
    {
        // Öğrenci Bilgileri
        public string OgrenciNo { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;
        public DateTime DogumTarihi { get; set; } = DateTime.Today;
        public string Cinsiyet { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;

        // Okul / Sınıf
        public int OkulId { get; set; }
        public int SinifId { get; set; }

        // Veli Bilgileri
        public string VeliAdSoyad { get; set; } = string.Empty;
        public string VeliTelefon { get; set; } = string.Empty;
        public string VeliYakinlik { get; set; } = string.Empty;

        // Ücret Durumu
        public bool Indirimli { get; set; }
        public bool Ucretsiz { get; set; }
        public decimal IndirimOrani { get; set; }
        public decimal? OzelFiyat { get; set; }
        public string? UcretAciklama { get; set; }

        // Ödeme Planı
        public string OdemeSekli { get; set; } = "Taksitli";
        public int TaksitSayisi { get; set; } = 8;
        public int BaslangicAyi { get; set; } = 10;
        public int OdemeGunu { get; set; } = 10;

        // Ekran listeleri
        public List<Okul> Okullar { get; set; } = new();
        public List<Sinif> Siniflar { get; set; } = new();
        public List<OkulUcreti> OkulUcretleri { get; set; } = new();
    }
}
