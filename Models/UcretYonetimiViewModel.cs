namespace OkulTakipSistemi.Models
{
    public class UcretYonetimiViewModel
    {
        // Seçimler
        public int? SeciliOkulId { get; set; }

        public int? SeciliKayitId { get; set; }


        // Okullar
        public List<Okul> Okullar { get; set; } = new();


        // Seçilen okulun öğrencileri
        public List<OgrenciKaydi> OgrenciKayitlari { get; set; } = new();


        // Seçilen öğrenci
        public OgrenciKaydi? SeciliKayit { get; set; }


        // Okulun normal ücreti
        public OkulUcreti? OkulUcreti { get; set; }


        // Öğrencinin özel ücreti
        public OgrenciOzelUcret? OzelUcret { get; set; }


        // Ödeme planı
        public List<AylikBorc> AylikBorclar { get; set; } = new();


        // Ödeme bilgileri
        public string OdemeSekli { get; set; } = "Taksitli";

        public int TaksitSayisi { get; set; } = 10;


        // Ücret tipi
        public string UcretTipi { get; set; } = "Normal";


        // Toplam uygulanacak ücret
        public decimal UygulanacakUcret
        {
            get
            {
                if (UcretTipi == "Ozel" &&
                    OzelUcret != null)
                {
                    return OzelUcret.OzelUcret;
                }

                return OkulUcreti?.UygulanacakStandartVeliUcreti ?? 0;
            }
        }


        // Toplam borç
        public decimal ToplamBorc =>
            AylikBorclar.Sum(x => x.Tutar);


        // Ödenen tutar
        public decimal OdenenTutar =>
            AylikBorclar.Sum(x => x.OdenenTutar);


        // Kalan bakiye
        public decimal KalanBakiye =>
            Math.Max(ToplamBorc - OdenenTutar, 0);


        // Toplam taksit
        public int ToplamTaksit =>
            AylikBorclar.Count;


        // Ödenen taksit
        public int OdenenTaksit =>
            AylikBorclar.Count(x => x.Odendi);


        // Kalan taksit
        public int KalanTaksit =>
            Math.Max(ToplamTaksit - OdenenTaksit, 0);
    }
}
