using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class OdemeController : Controller
    {
        private readonly AppDbContext _context;

        public OdemeController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // ÖDEME PANELİ
        // ============================================================

        [HttpGet]
        public IActionResult Index(int? ogrenciId)
        {
            // Öğrencileri getir
            var ogrenciler = _context.Ogrenciler
                .OrderBy(x => x.AdSoyad)
                .ToList();

            ViewBag.Ogrenciler = ogrenciler;
            ViewBag.SeciliOgrenciId = ogrenciId;

            // Öğrenci seçilmediyse boş ödeme listesi göster
            if (ogrenciId == null)
            {
                ViewBag.Borclar = new List<AylikBorc>();
                ViewBag.Odemeler = new List<Odeme>();
                ViewBag.ToplamBorc = 0m;
                ViewBag.ToplamOdenen = 0m;
                ViewBag.ToplamKalan = 0m;

                return View(new List<Odeme>());
            }

            // Seçilen öğrenciyi bul
            var ogrenci = _context.Ogrenciler
                .FirstOrDefault(x => x.Id == ogrenciId);

            if (ogrenci == null)
            {
                return NotFound();
            }

            ViewBag.Ogrenci = ogrenci;

            // Öğrencinin aktif okul kaydını getir
            var kayit = _context.OgrenciKayitlari
                .Include(x => x.Okul)
                .Include(x => x.Sinif)
                .FirstOrDefault(x =>
                    x.OgrenciId == ogrenciId &&
                    x.Aktif);

            ViewBag.Kayit = kayit;

            // ========================================================
            // AYLIK BORÇLAR
            // ========================================================

            var borclar = _context.AylikBorclar
                .Include(x => x.OgrenciKaydi)
                .Where(x =>
                    x.OgrenciKaydi != null &&
                    x.OgrenciKaydi.OgrenciId == ogrenciId)
                .OrderBy(x => x.Yil)
                .ThenBy(x => x.Ay)
                .ToList();

            ViewBag.Borclar = borclar;

            // ========================================================
            // ÖDEMELER
            // ========================================================

            var odemeler = _context.Odemeler
                .Include(x => x.Kullanici)
                .Include(x => x.OdemeDagilimlari)
                    .ThenInclude(x => x.AylikBorc)
                .Where(x => x.OgrenciId == ogrenciId)
                .OrderByDescending(x => x.OdemeTarihi)
                .ToList();

            ViewBag.Odemeler = odemeler;

            // ========================================================
            // TOPLAM HESAPLAR
            // ========================================================

            decimal toplamBorc = borclar.Sum(x => x.Tutar);

            decimal toplamOdenen = borclar.Sum(x => x.OdenenTutar);

            decimal toplamKalan = borclar.Sum(x => x.KalanTutar);

            ViewBag.ToplamBorc = toplamBorc;
            ViewBag.ToplamOdenen = toplamOdenen;
            ViewBag.ToplamKalan = toplamKalan;

            // View artık doğrudan Odeme modeli kullanıyor
            return View(odemeler);
        }


        // ============================================================
        // YENİ ÖDEME SAYFASI
        // ============================================================

        [HttpGet]
        public IActionResult Create(int? ogrenciId)
        {
            var ogrenciler = _context.Ogrenciler
                .OrderBy(x => x.AdSoyad)
                .ToList();

            ViewBag.Ogrenciler = ogrenciler;

            var model = new Odeme
            {
                OdemeTarihi = DateTime.Today,
                Durum = "Ödendi"
            };

            if (ogrenciId.HasValue)
            {
                model.OgrenciId = ogrenciId.Value;
            }

            return View(model);
        }


        // ============================================================
        // ÖDEME KAYDET
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Odeme odeme)
        {
            // Giriş yapan kullanıcı
            var kullaniciId =
                HttpContext.Session.GetInt32("KullaniciId");

            if (kullaniciId == null)
            {
                TempData["Hata"] =
                    "Oturum bilgisi bulunamadı. Lütfen tekrar giriş yapın.";

                return RedirectToAction(
                    "AdminLogin",
                    "Account");
            }

            // ========================================================
            // TEMEL KONTROLLER
            // ========================================================

            if (odeme.OgrenciId <= 0)
            {
                ModelState.AddModelError(
                    "OgrenciId",
                    "Lütfen öğrenci seçiniz.");
            }

            if (odeme.Tutar <= 0)
            {
                ModelState.AddModelError(
                    "Tutar",
                    "Ödeme tutarı 0'dan büyük olmalıdır.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Ogrenciler = _context.Ogrenciler
                    .OrderBy(x => x.AdSoyad)
                    .ToList();

                return View(odeme);
            }

            // ========================================================
            // ÖĞRENCİYİ KONTROL ET
            // ========================================================

            var ogrenci = _context.Ogrenciler
                .FirstOrDefault(x => x.Id == odeme.OgrenciId);

            if (ogrenci == null)
            {
                ModelState.AddModelError(
                    "OgrenciId",
                    "Seçilen öğrenci bulunamadı.");

                ViewBag.Ogrenciler = _context.Ogrenciler
                    .OrderBy(x => x.AdSoyad)
                    .ToList();

                return View(odeme);
            }

            // ========================================================
            // AÇIK BORÇLARI BUL
            // ========================================================

            var borclar = _context.AylikBorclar
                .Include(x => x.OgrenciKaydi)
                .Where(x =>
                    x.OgrenciKaydi != null &&
                    x.OgrenciKaydi.OgrenciId == odeme.OgrenciId &&
                    x.OdenenTutar < x.Tutar)
                .OrderBy(x => x.Yil)
                .ThenBy(x => x.Ay)
                .ToList();

            // ========================================================
            // BORÇ YOKSA
            // ========================================================

            if (!borclar.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Bu öğrencinin ödenmemiş herhangi bir borcu bulunmuyor. Önce ödeme planı oluşturmalısınız.");

                ViewBag.Ogrenciler = _context.Ogrenciler
                    .OrderBy(x => x.AdSoyad)
                    .ToList();

                return View(odeme);
            }

            // ========================================================
            // TOPLAM KALAN BORÇ
            // ========================================================

            decimal toplamKalan = borclar.Sum(
                x => x.Tutar - x.OdenenTutar);

            // Fazla ödeme kontrolü
            if (odeme.Tutar > toplamKalan)
            {
                ModelState.AddModelError(
                    "Tutar",
                    $"Ödeme toplam kalan borçtan fazla olamaz. Kalan borç: {toplamKalan:N2} ₺");

                ViewBag.Ogrenciler = _context.Ogrenciler
                    .OrderBy(x => x.AdSoyad)
                    .ToList();

                return View(odeme);
            }

            // ========================================================
            // ÖDEME KAYDI
            // ========================================================

            odeme.KullaniciId = kullaniciId.Value;

            if (string.IsNullOrWhiteSpace(odeme.Durum))
            {
                odeme.Durum = "Ödendi";
            }

            _context.Odemeler.Add(odeme);

            _context.SaveChanges();

            // ========================================================
            // ÖDEMEYİ BORÇLARA DAĞIT
            // ========================================================

            decimal kalanOdeme = odeme.Tutar;

            foreach (var borc in borclar)
            {
                if (kalanOdeme <= 0)
                {
                    break;
                }

                decimal kalanBorc =
                    borc.Tutar - borc.OdenenTutar;

                if (kalanBorc <= 0)
                {
                    continue;
                }

                decimal dagitilacak =
                    Math.Min(kalanOdeme, kalanBorc);

                var dagilim = new OdemeDagilimi
                {
                    OdemeId = odeme.Id,
                    AylikBorcId = borc.Id,
                    Tutar = dagitilacak
                };

                borc.OdenenTutar += dagitilacak;

                _context.OdemeDagilimlari.Add(dagilim);

                kalanOdeme -= dagitilacak;
            }

            _context.SaveChanges();

            TempData["Basarili"] =
                $"{odeme.Tutar:N2} ₺ ödeme kaydedildi ve borçlara dağıtıldı.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    ogrenciId = odeme.OgrenciId
                });
        }


        // ============================================================
        // ÖDEME SİL
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var odeme = _context.Odemeler
                .Include(x => x.OdemeDagilimlari)
                    .ThenInclude(x => x.AylikBorc)
                .FirstOrDefault(x => x.Id == id);

            if (odeme == null)
            {
                return NotFound();
            }

            int ogrenciId = odeme.OgrenciId;

            // Ödeme dağılımlarını geri al
            foreach (var dagilim in odeme.OdemeDagilimlari)
            {
                if (dagilim.AylikBorc != null)
                {
                    dagilim.AylikBorc.OdenenTutar -=
                        dagilim.Tutar;

                    if (dagilim.AylikBorc.OdenenTutar < 0)
                    {
                        dagilim.AylikBorc.OdenenTutar = 0;
                    }
                }
            }

            // Ödemeyi sil
            _context.Odemeler.Remove(odeme);

            _context.SaveChanges();

            TempData["Basarili"] =
                "Ödeme silindi ve borç tutarı geri alındı.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    ogrenciId = ogrenciId
                });
        }


        // ============================================================
        // ÖĞRENCİNİN BORÇLARINI GETİR
        // ============================================================

        [HttpGet]
        public IActionResult Borclar(int ogrenciId)
        {
            var borclar = _context.AylikBorclar
                .Include(x => x.OgrenciKaydi)
                .Where(x =>
                    x.OgrenciKaydi != null &&
                    x.OgrenciKaydi.OgrenciId == ogrenciId)
                .OrderBy(x => x.Yil)
                .ThenBy(x => x.Ay)
                .ToList();

            return Json(
                borclar.Select(x => new
                {
                    id = x.Id,
                    yil = x.Yil,
                    ay = x.Ay,
                    tutar = x.Tutar,
                    odenen = x.OdenenTutar,
                    kalan = x.KalanTutar,
                    odendi = x.Odendi
                })
            );
        }
    }
}