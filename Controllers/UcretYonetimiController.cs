using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class UcretYonetimiController : Controller
    {
        private readonly AppDbContext _context;

        public UcretYonetimiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(int? okulId, int? kayitId)
        {
            if (!YoneticiMi())
                return RedirectToAction("Login", "Account");

            var model = new UcretYonetimiViewModel
            {
                SeciliOkulId = okulId,
                SeciliKayitId = kayitId,

                Okullar = _context.Okullar
                    .OrderBy(x => x.Ad)
                    .ToList()
            };

            // Okul seçildiyse sadece o okulun öğrencilerini getir
            if (okulId.HasValue && okulId.Value > 0)
            {
                model.OgrenciKayitlari = _context.OgrenciKayitlari
                    .Include(x => x.Ogrenci)
                    .Include(x => x.Sinif)
                    .Where(x => x.Aktif &&
                                x.OkulId == okulId.Value)
                    .OrderBy(x => x.Ogrenci!.AdSoyad)
                    .ToList();

                model.OkulUcreti = _context.OkulUcretleri
                    .Where(x => x.OkulId == okulId.Value)
                    .OrderByDescending(x => x.GecerlilikTarihi)
                    .ThenByDescending(x => x.Id)
                    .FirstOrDefault();
            }

            // Öğrenci seçildiyse bilgilerini getir
            if (kayitId.HasValue && kayitId.Value > 0)
            {
                var kayit = _context.OgrenciKayitlari
                    .Include(x => x.Ogrenci)
                    .Include(x => x.Okul)
                    .Include(x => x.Sinif)
                    .FirstOrDefault(x =>
                        x.Id == kayitId.Value &&
                        x.Aktif);

                if (kayit != null)
                {
                    model.SeciliKayit = kayit;
                    model.SeciliOkulId = kayit.OkulId;

                    // Güvenlik için öğrencinin okulunun öğrencilerini getir
                    model.OgrenciKayitlari = _context.OgrenciKayitlari
                        .Include(x => x.Ogrenci)
                        .Include(x => x.Sinif)
                        .Where(x => x.Aktif &&
                                    x.OkulId == kayit.OkulId)
                        .OrderBy(x => x.Ogrenci!.AdSoyad)
                        .ToList();

                    model.OkulUcreti = _context.OkulUcretleri
                        .Where(x => x.OkulId == kayit.OkulId)
                        .OrderByDescending(x => x.GecerlilikTarihi)
                        .ThenByDescending(x => x.Id)
                        .FirstOrDefault();

                    model.OzelUcret = _context.OgrenciOzelUcretler
                        .FirstOrDefault(x =>
                            x.OgrenciKaydiId == kayit.Id);

                    model.AylikBorclar = _context.AylikBorclar
                        .Where(x => x.OgrenciKaydiId == kayit.Id)
                        .OrderBy(x => x.Yil)
                        .ThenBy(x => x.Ay)
                        .ToList();

                    // Özel ücret varsa varsayılan olarak onu seç
                    if (model.OzelUcret != null)
                        model.UcretTipi = "Ozel";

                    // Mevcut ödeme planından taksit sayısını bul
                    if (model.AylikBorclar.Any())
                    {
                        model.TaksitSayisi =
                            model.AylikBorclar.Count;
                    }
                }
            }

            return View(model);
        }


        // OKUL ÜCRETİNİ KAYDET
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OkulUcretiKaydet(
            int okulId,
            decimal normalUcret)
        {
            if (!YoneticiMi())
                return RedirectToAction("Login", "Account");

            if (okulId <= 0 || normalUcret <= 0)
            {
                TempData["Hata"] =
                    "Okul ve normal ücret bilgisi zorunludur.";

                return RedirectToAction(nameof(Index));
            }

            var mevcut = _context.OkulUcretleri
                .Where(x => x.OkulId == okulId)
                .OrderByDescending(x => x.GecerlilikTarihi)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            if (mevcut == null)
            {
                mevcut = new OkulUcreti
                {
                    OkulId = okulId,
                    OlusturmaTarihi = DateTime.Now
                };

                _context.OkulUcretleri.Add(mevcut);
            }

            mevcut.StandartVeliUcreti = normalUcret;
            mevcut.ToplamUcret = normalUcret;
            mevcut.TaksitAySayisi = 1;
            mevcut.OdemeSekli = "Belirlenmedi";
            mevcut.BaslangicAyi = 9;
            mevcut.GecerlilikTarihi =
                new DateTime(DateTime.Today.Year, 9, 1);

            _context.SaveChanges();

            TempData["Basarili"] =
                "Okulun normal ücret bilgisi kaydedildi.";

            return RedirectToAction(nameof(Index),
                new { okulId });
        }


        // ÖZEL ÜCRETİ KAYDET
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OzelUcretKaydet(
            int kayitId,
            decimal ozelUcret,
            string? aciklama)
        {
            if (!YoneticiMi())
                return RedirectToAction("Login", "Account");

            var kayit = AktifKaydiGetir(kayitId);

            if (kayit == null)
                return NotFound();

            if (ozelUcret <= 0)
            {
                TempData["Hata"] =
                    "Özel ücret 0'dan büyük olmalıdır.";

                return RedirectToAction(nameof(Index),
                    new
                    {
                        okulId = kayit.OkulId,
                        kayitId
                    });
            }

            var mevcut = _context.OgrenciOzelUcretler
                .FirstOrDefault(x =>
                    x.OgrenciKaydiId == kayitId);

            if (mevcut == null)
            {
                mevcut = new OgrenciOzelUcret
                {
                    OgrenciKaydiId = kayitId,
                    OlusturmaTarihi = DateTime.Now
                };

                _context.OgrenciOzelUcretler.Add(mevcut);
            }

            mevcut.OzelUcret = ozelUcret;
            mevcut.Aciklama = aciklama;
            mevcut.GuncellemeTarihi = DateTime.Now;

            _context.SaveChanges();

            TempData["Basarili"] =
                "Öğrenciye özel ücret kaydedildi.";

            return RedirectToAction(nameof(Index),
                new
                {
                    okulId = kayit.OkulId,
                    kayitId
                });
        }


        // ÖDEME PLANINI OLUŞTUR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OdemePlaniOlustur(
            int kayitId,
            string odemeSekli,
            int taksitSayisi,
            string ucretTipi)
        {
            if (!YoneticiMi())
                return RedirectToAction("Login", "Account");

            var kayit = AktifKaydiGetir(kayitId);

            if (kayit == null)
                return NotFound();

            if (_context.AylikBorclar.Any(
                x => x.OgrenciKaydiId == kayitId))
            {
                TempData["Hata"] =
                    "Bu öğrenci için zaten ödeme planı oluşturulmuş.";

                return RedirectToAction(nameof(Index),
                    new
                    {
                        okulId = kayit.OkulId,
                        kayitId
                    });
            }

            var okulUcreti = _context.OkulUcretleri
                .Where(x => x.OkulId == kayit.OkulId)
                .OrderByDescending(x => x.GecerlilikTarihi)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            if (okulUcreti == null)
            {
                TempData["Hata"] =
                    "Önce okulun normal ücretini giriniz.";

                return RedirectToAction(nameof(Index),
                    new
                    {
                        okulId = kayit.OkulId,
                        kayitId
                    });
            }

            decimal uygulanacakUcret;

            var ozelUcret = _context.OgrenciOzelUcretler
                .FirstOrDefault(x =>
                    x.OgrenciKaydiId == kayitId);

            if (ucretTipi == "Ozel")
            {
                if (ozelUcret == null)
                {
                    TempData["Hata"] =
                        "Özel ücret seçildi ancak öğrenciye özel ücret girilmemiş.";

                    return RedirectToAction(nameof(Index),
                        new
                        {
                            okulId = kayit.OkulId,
                            kayitId
                        });
                }

                uygulanacakUcret = ozelUcret.OzelUcret;
            }
            else
            {
                uygulanacakUcret =
                    okulUcreti.UygulanacakStandartVeliUcreti;
            }

            if (odemeSekli == "Pesin")
                taksitSayisi = 1;

            var aylikTutar = Math.Round(
                uygulanacakUcret / taksitSayisi, 2);

            var borclar = new List<AylikBorc>();

            for (var i = 0; i < taksitSayisi; i++)
            {
                var tarih = DateTime.Today
                    .AddMonths(i);

                var tutar = i == taksitSayisi - 1
                    ? uygulanacakUcret -
                      (aylikTutar * (taksitSayisi - 1))
                    : aylikTutar;

                borclar.Add(new AylikBorc
                {
                    OgrenciKaydiId = kayitId,
                    Yil = tarih.Year,
                    Ay = tarih.Month,
                    Tutar = tutar,
                    Aciklama =
                        $"{i + 1}. taksit"
                });
            }

            _context.AylikBorclar.AddRange(borclar);
            _context.SaveChanges();

            TempData["Basarili"] =
                "Ödeme planı başarıyla oluşturuldu.";

            return RedirectToAction(nameof(Index),
                new
                {
                    okulId = kayit.OkulId,
                    kayitId
                });
        }



        // ÖDEME PLANINI GÜNCELLE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OdemePlaniGuncelle(
            int kayitId,
            string odemeSekli,
            int taksitSayisi,
            string ucretTipi)
        {
            if (!YoneticiMi())
                return RedirectToAction("Login", "Account");

            var kayit = AktifKaydiGetir(kayitId);

            if (kayit == null)
                return NotFound();

            var mevcutBorclar = _context.AylikBorclar
                .Where(x => x.OgrenciKaydiId == kayitId)
                .OrderBy(x => x.Yil)
                .ThenBy(x => x.Ay)
                .ToList();

            // Herhangi bir ödeme yapıldıysa plan değiştirilemez
            if (mevcutBorclar.Any(x => x.OdenenTutar > 0))
            {
                TempData["Hata"] =
                    "Bu ödeme planında ödeme yapıldığı için plan değiştirilemez.";

                return RedirectToAction(nameof(Index),
                    new
                    {
                        okulId = kayit.OkulId,
                        kayitId
                    });
            }

            var okulUcreti = _context.OkulUcretleri
                .Where(x => x.OkulId == kayit.OkulId)
                .OrderByDescending(x => x.GecerlilikTarihi)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            if (okulUcreti == null)
            {
                TempData["Hata"] =
                    "Önce okulun normal ücretini giriniz.";

                return RedirectToAction(nameof(Index),
                    new
                    {
                        okulId = kayit.OkulId,
                        kayitId
                    });
            }

            decimal uygulanacakUcret;

            var ozelUcret = _context.OgrenciOzelUcretler
                .FirstOrDefault(x =>
                    x.OgrenciKaydiId == kayitId);

            if (ucretTipi == "Ozel")
            {
                if (ozelUcret == null)
                {
                    TempData["Hata"] =
                        "Özel ücret seçildi ancak öğrenciye özel ücret girilmemiş.";

                    return RedirectToAction(nameof(Index),
                        new
                        {
                            okulId = kayit.OkulId,
                            kayitId
                        });
                }

                uygulanacakUcret = ozelUcret.OzelUcret;
            }
            else
            {
                uygulanacakUcret =
                    okulUcreti.UygulanacakStandartVeliUcreti;
            }

            if (odemeSekli == "Pesin")
                taksitSayisi = 1;

            if (taksitSayisi < 1 || taksitSayisi > 24)
                taksitSayisi = 10;

            // Eski, ödeme yapılmamış planı sil
            _context.AylikBorclar.RemoveRange(mevcutBorclar);

            var aylikTutar = Math.Round(
                uygulanacakUcret / taksitSayisi, 2);

            var borclar = new List<AylikBorc>();

            for (var i = 0; i < taksitSayisi; i++)
            {
                var tarih = DateTime.Today.AddMonths(i);

                var tutar = i == taksitSayisi - 1
                    ? uygulanacakUcret -
                      (aylikTutar * (taksitSayisi - 1))
                    : aylikTutar;

                borclar.Add(new AylikBorc
                {
                    OgrenciKaydiId = kayitId,
                    Yil = tarih.Year,
                    Ay = tarih.Month,
                    Tutar = tutar,
                    Aciklama = $"{i + 1}. taksit"
                });
            }

            _context.AylikBorclar.AddRange(borclar);
            _context.SaveChanges();

            TempData["Basarili"] =
                "Ödeme planı başarıyla güncellendi.";

            return RedirectToAction(nameof(Index),
                new
                {
                    okulId = kayit.OkulId,
                    kayitId
                });
        }


        private OgrenciKaydi? AktifKaydiGetir(int kayitId)
        {
            return _context.OgrenciKayitlari
                .Include(x => x.Ogrenci)
                .Include(x => x.Okul)
                .Include(x => x.Sinif)
                .FirstOrDefault(x =>
                    x.Id == kayitId && x.Aktif);
        }

        private bool YoneticiMi()
        {
            return HttpContext.Session.GetString("Rol") == "Admin";
        }
    }
}
