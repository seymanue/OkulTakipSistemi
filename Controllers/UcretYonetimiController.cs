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
        public IActionResult Index()
        {
            if (!YoneticiMi())
            {
                return RedirectToAction("Login", "Account");
            }

            var tarifeler = _context.OkulUcretleri
                .Include(x => x.Okul)
                .OrderByDescending(x => x.GecerlilikTarihi)
                .ThenByDescending(x => x.Id)
                .ToList();

            var ozelUcretler = _context.OgrenciOzelUcretler
                .ToDictionary(x => x.OgrenciKaydiId);

            var planliKayitlar = _context.AylikBorclar
                .Select(x => x.OgrenciKaydiId)
                .Distinct()
                .ToHashSet();

            var ogrenciler = _context.OgrenciKayitlari
                .Include(x => x.Ogrenci)
                .Include(x => x.Okul)
                .Include(x => x.Sinif)
                .Where(x => x.Aktif)
                .OrderBy(x => x.Ogrenci!.AdSoyad)
                .ToList()
                .Select(kayit => new UcretYonetimiOgrenciSatiri
                {
                    Kayit = kayit,
                    StandartVeliUcreti = EnGuncelTarife(tarifeler, kayit.OkulId)
                        ?.UygulanacakStandartVeliUcreti,
                    OzelUcret = ozelUcretler.GetValueOrDefault(kayit.Id),
                    OdemePlaniVar = planliKayitlar.Contains(kayit.Id)
                })
                .ToList();

            return View(new UcretYonetimiViewModel
            {
                Tarifeler = tarifeler,
                Okullar = _context.Okullar.OrderBy(x => x.Ad).ToList(),
                Ogrenciler = ogrenciler
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TarifeEkle(
            [Bind(Prefix = "YeniTarife")] OkulUcreti yeniTarife)
        {
            if (!YoneticiMi())
            {
                return RedirectToAction("Login", "Account");
            }

            if (yeniTarife.OkulId <= 0)
            {
                TempData["Hata"] = "Lütfen okul seçiniz.";
                return RedirectToAction(nameof(Index));
            }

            if (yeniTarife.KurumIcinBelirlenenUcret is < 0 ||
                yeniTarife.StandartVeliUcreti is null or <= 0)
            {
                TempData["Hata"] = "Kurum ücreti negatif olamaz ve standart veli ücreti 0'dan büyük olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            if (yeniTarife.OdemeSekli != "Pesin" &&
                yeniTarife.OdemeSekli != "Taksitli")
            {
                TempData["Hata"] = "Geçerli bir ödeme şekli seçiniz.";
                return RedirectToAction(nameof(Index));
            }

            if (yeniTarife.OdemeSekli == "Pesin")
            {
                yeniTarife.TaksitAySayisi = 1;
            }
            else if (yeniTarife.TaksitAySayisi is < 1 or > 24)
            {
                TempData["Hata"] = "Taksit sayısı 1 ile 24 arasında olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            if (yeniTarife.BaslangicAyi is < 1 or > 12)
            {
                TempData["Hata"] = "Başlangıç ayı geçersiz.";
                return RedirectToAction(nameof(Index));
            }

            if (yeniTarife.GecerlilikTarihi == default)
            {
                yeniTarife.GecerlilikTarihi = new DateTime(
                    DateTime.Today.Year,
                    yeniTarife.BaslangicAyi,
                    1);
            }

            yeniTarife.ToplamUcret = yeniTarife.StandartVeliUcreti.Value;
            yeniTarife.OlusturmaTarihi = DateTime.Now;

            _context.OkulUcretleri.Add(yeniTarife);
            _context.SaveChanges();

            TempData["Basarili"] = "Yeni okul tarifesi kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult OzelUcret(int kayitId)
        {
            if (!YoneticiMi())
            {
                return RedirectToAction("Login", "Account");
            }

            var kayit = AktifKaydiGetir(kayitId);
            if (kayit == null)
            {
                return NotFound();
            }

            var model = _context.OgrenciOzelUcretler
                .FirstOrDefault(x => x.OgrenciKaydiId == kayitId)
                ?? new OgrenciOzelUcret { OgrenciKaydiId = kayitId };

            ViewBag.Kayit = kayit;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OzelUcret(OgrenciOzelUcret model)
        {
            if (!YoneticiMi())
            {
                return RedirectToAction("Login", "Account");
            }

            if (AktifKaydiGetir(model.OgrenciKaydiId) == null)
            {
                return NotFound();
            }

            if (model.OzelUcret <= 0)
            {
                ModelState.AddModelError(nameof(model.OzelUcret), "Özel ücret 0'dan büyük olmalıdır.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Kayit = AktifKaydiGetir(model.OgrenciKaydiId);
                return View(model);
            }

            var mevcut = _context.OgrenciOzelUcretler
                .FirstOrDefault(x => x.OgrenciKaydiId == model.OgrenciKaydiId);

            if (mevcut == null)
            {
                model.OlusturmaTarihi = DateTime.Now;
                model.GuncellemeTarihi = DateTime.Now;
                _context.OgrenciOzelUcretler.Add(model);
            }
            else
            {
                mevcut.OzelUcret = model.OzelUcret;
                mevcut.Aciklama = model.Aciklama;
                mevcut.GuncellemeTarihi = DateTime.Now;
            }

            _context.SaveChanges();
            TempData["Basarili"] = "Öğrenciye özel ücret kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OdemePlaniOlustur(int kayitId)
        {
            if (!YoneticiMi())
            {
                return RedirectToAction("Login", "Account");
            }

            var kayit = AktifKaydiGetir(kayitId);
            if (kayit == null)
            {
                return NotFound();
            }

            if (_context.AylikBorclar.Any(x => x.OgrenciKaydiId == kayitId))
            {
                TempData["Hata"] = "Bu öğrenci için mevcut bir ödeme planı bulunuyor; geçmiş plan değiştirilemez.";
                return RedirectToAction(nameof(Index));
            }

            var tarife = _context.OkulUcretleri
                .Where(x => x.OkulId == kayit.OkulId)
                .OrderByDescending(x => x.GecerlilikTarihi)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            if (tarife == null)
            {
                TempData["Hata"] = "Önce öğrencinin okuluna standart veli ücreti içeren bir tarife tanımlayın.";
                return RedirectToAction(nameof(Index));
            }

            var ozelUcret = _context.OgrenciOzelUcretler
                .FirstOrDefault(x => x.OgrenciKaydiId == kayitId);

            var uygulanacakUcret = ozelUcret?.OzelUcret ??
                tarife.UygulanacakStandartVeliUcreti;

            if (uygulanacakUcret <= 0)
            {
                TempData["Hata"] = "Uygulanacak ücret 0'dan büyük olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            var taksitSayisi = tarife.OdemeSekli == "Pesin"
                ? 1
                : Math.Clamp(tarife.TaksitAySayisi, 1, 24);
            var aylikTutar = Math.Round(uygulanacakUcret / taksitSayisi, 2);
            var baslangicTarihi = new DateTime(
                tarife.GecerlilikTarihi == default ? DateTime.Today.Year : tarife.GecerlilikTarihi.Year,
                tarife.BaslangicAyi is >= 1 and <= 12 ? tarife.BaslangicAyi : DateTime.Today.Month,
                1);
            var borclar = new List<AylikBorc>();

            for (var taksit = 0; taksit < taksitSayisi; taksit++)
            {
                var tarih = baslangicTarihi.AddMonths(taksit);
                var tutar = taksit == taksitSayisi - 1
                    ? uygulanacakUcret - (aylikTutar * (taksitSayisi - 1))
                    : aylikTutar;

                borclar.Add(new AylikBorc
                {
                    OgrenciKaydiId = kayitId,
                    Yil = tarih.Year,
                    Ay = tarih.Month,
                    Tutar = tutar,
                    Aciklama = "Ücret Yönetimi üzerinden otomatik oluşturulan ödeme planı"
                });
            }

            _context.AylikBorclar.AddRange(borclar);
            _context.SaveChanges();

            TempData["Basarili"] = "Ödeme planı uygulanan ücret üzerinden oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        private OgrenciKaydi? AktifKaydiGetir(int kayitId)
        {
            return _context.OgrenciKayitlari
                .Include(x => x.Ogrenci)
                .Include(x => x.Okul)
                .Include(x => x.Sinif)
                .FirstOrDefault(x => x.Id == kayitId && x.Aktif);
        }

        private static OkulUcreti? EnGuncelTarife(
            IEnumerable<OkulUcreti> tarifeler,
            int okulId)
        {
            return tarifeler.FirstOrDefault(x => x.OkulId == okulId);
        }

        private bool YoneticiMi()
        {
            return HttpContext.Session.GetString("Rol") == "Admin";
        }
    }
}
