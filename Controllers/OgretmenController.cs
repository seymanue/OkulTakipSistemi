using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class OgretmenController : Controller
    {
        private readonly AppDbContext _context;

        public OgretmenController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // YÖNETİCİ - Öğretmenleri listele
        // ============================================================

        public IActionResult Index()
        {
            var ogretmenler = _context.Ogretmenler
                .Include(x => x.Kullanici)
                .ToList();

            return View(ogretmenler);
        }

        // ============================================================
        // YÖNETİCİ - Öğretmen ekleme
        // ============================================================

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Kullanicilar = _context.Kullanicilar.ToList();

            return View();
        }

        // ============================================================
        // YÖNETİCİ - Öğretmen kaydetme
        // ============================================================

        [HttpPost]
        public IActionResult Create(Ogretmen ogretmen)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Kullanicilar = _context.Kullanicilar.ToList();

                    return View(ogretmen);
                }

                var kullanici = _context.Kullanicilar
                    .FirstOrDefault(x =>
                        x.Id == ogretmen.KullaniciId);

                if (kullanici == null)
                {
                    ModelState.AddModelError(
                        "KullaniciId",
                        "Geçerli bir kullanıcı seçmelisiniz."
                    );

                    ViewBag.Kullanicilar =
                        _context.Kullanicilar.ToList();

                    return View(ogretmen);
                }

                var mevcutOgretmen = _context.Ogretmenler
                    .FirstOrDefault(x =>
                        x.KullaniciId == ogretmen.KullaniciId);

                if (mevcutOgretmen != null)
                {
                    ModelState.AddModelError(
                        "KullaniciId",
                        "Bu kullanıcı zaten bir öğretmene atanmış."
                    );

                    ViewBag.Kullanicilar =
                        _context.Kullanicilar.ToList();

                    return View(ogretmen);
                }

                _context.Ogretmenler.Add(ogretmen);

                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Kullanicilar =
                    _context.Kullanicilar.ToList();

                ModelState.AddModelError(
                    "",
                    "Kayıt sırasında hata oluştu: " +
                    ex.Message
                );

                return View(ogretmen);
            }
        }

        // ============================================================
        // YÖNETİCİ - Öğretmen silme
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x => x.Id == id);

            if (ogretmen == null)
                return NotFound();

            var kullaniciId = ogretmen.KullaniciId;

            var kullanici = _context.Kullanicilar
                .FirstOrDefault(x => x.Id == kullaniciId);

            // Ana admin hesabı hiçbir şekilde silinmesin.
            if (kullanici != null && kullanici.KullaniciAdi == "admin")
            {
                TempData["Hata"] =
                    "Ana admin kullanıcısı silinemez.";

                return RedirectToAction(nameof(Index));
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 1. Öğretmene ait yoklama detaylarını sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM YoklamaDetaylari
                    WHERE YoklamaId IN (
                        SELECT Id
                        FROM Yoklamalar
                        WHERE OgretmenId = {id}
                    )
                    """);

                // 2. Öğretmene ait yoklamaları sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM Yoklamalar
                    WHERE OgretmenId = {id}
                    """);

                // 3. Öğretmen-sınıf bağlantılarını sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM OgretmenSiniflar
                    WHERE OgretmenId = {id}
                    """);

                // 4. Öğretmeni sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM Ogretmenler
                    WHERE Id = {id}
                    """);

                // 5. Öğretmenin kullanıcı hesabını sil
                if (kullaniciId > 0)
                {
                    _context.Database.ExecuteSqlInterpolated($"""
                        DELETE FROM Kullanicilar
                        WHERE Id = {kullaniciId}
                        """);
                }

                transaction.Commit();

                TempData["Basarili"] =
                    "Öğretmen ve kullanıcı hesabı başarıyla silindi.";
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                Console.WriteLine("================================");
                Console.WriteLine("ÖĞRETMEN SİLME HATASI");
                Console.WriteLine("ÖĞRETMEN ID: " + id);
                Console.WriteLine("HATA: " + ex.Message);
                Console.WriteLine("================================");

                TempData["Hata"] =
                    "Öğretmen silinemedi. İşlem geri alındı.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // ÖĞRETMEN - Kendisine atanmış sınıflar
        // ============================================================

        public IActionResult Siniflarim()
        {
            var kullaniciAdi =
                HttpContext.Session.GetString("KullaniciAdi");

            if (string.IsNullOrEmpty(kullaniciAdi))
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var kullanici = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.KullaniciAdi == kullaniciAdi);

            if (kullanici == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x =>
                    x.KullaniciId == kullanici.Id);

            if (ogretmen == null)
            {
                return Unauthorized();
            }

            var siniflar = _context.OgretmenSiniflar
                .Where(x =>
                    x.OgretmenId == ogretmen.Id)
                .Include(x => x.Sinif)
                .ThenInclude(x => x.Okul)
                .Select(x => x.Sinif)
                .Where(x => x != null)
                .ToList();

            return View(siniflar);
        }

        // ============================================================
        // ÖĞRETMEN - Seçilen sınıftaki öğrenciler
        // ============================================================

        [HttpGet]
        public IActionResult SinifOgrencileri(int id)
        {
            var kullaniciAdi =
                HttpContext.Session.GetString("KullaniciAdi");

            if (string.IsNullOrEmpty(kullaniciAdi))
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var kullanici = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.KullaniciAdi == kullaniciAdi);

            if (kullanici == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x =>
                    x.KullaniciId == kullanici.Id);

            if (ogretmen == null)
            {
                return Unauthorized();
            }

            // --------------------------------------------------------
            // Öğretmenin bu sınıfa atanmış olup olmadığını kontrol et
            // --------------------------------------------------------

            var atama = _context.OgretmenSiniflar
                .FirstOrDefault(x =>
                    x.OgretmenId == ogretmen.Id &&
                    x.SinifId == id);

            if (atama == null)
            {
                return Unauthorized();
            }

            // --------------------------------------------------------
            // Sınıfı getir
            // --------------------------------------------------------

            var sinif = _context.Siniflar
                .Include(x => x.Okul)
                .FirstOrDefault(x =>
                    x.Id == id);

            if (sinif == null)
            {
                return NotFound();
            }

            // --------------------------------------------------------
            // Sınıftaki öğrencileri getir
            // --------------------------------------------------------

            var ogrenciler = _context.Ogrenciler
                .Where(x =>
                    x.SinifId.HasValue &&
                    x.SinifId.Value == id)
                .OrderBy(x => x.AdSoyad)
                .ToList();

            ViewBag.Sinif = sinif;

            return View(ogrenciler);
        }

        // ============================================================
        // ÖĞRETMEN - Yoklama ekranı
        // ============================================================

        public IActionResult Yoklama()
        {
            var kullaniciAdi =
                HttpContext.Session.GetString("KullaniciAdi");

            if (string.IsNullOrEmpty(kullaniciAdi))
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var kullanici = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.KullaniciAdi == kullaniciAdi);

            if (kullanici == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x =>
                    x.KullaniciId == kullanici.Id);

            if (ogretmen == null)
            {
                return Unauthorized();
            }

            var siniflar = _context.OgretmenSiniflar
                .Where(x =>
                    x.OgretmenId == ogretmen.Id)
                .Include(x => x.Sinif)
                .ThenInclude(x => x.Okul)
                .Select(x => x.Sinif)
                .Where(x => x != null)
                .ToList();

            return View(siniflar);
        }

        // ============================================================
        // ÖĞRETMEN - Seçilen sınıfın öğrencilerini getir
        // YOKLAMA
        // ============================================================

        [HttpGet]
        public IActionResult YoklamaAl(int id)
        {
            var kullaniciAdi =
                HttpContext.Session.GetString("KullaniciAdi");

            if (string.IsNullOrEmpty(kullaniciAdi))
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var kullanici = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.KullaniciAdi == kullaniciAdi);

            if (kullanici == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x =>
                    x.KullaniciId == kullanici.Id);

            if (ogretmen == null)
            {
                return Unauthorized();
            }

            var atama = _context.OgretmenSiniflar
                .FirstOrDefault(x =>
                    x.OgretmenId == ogretmen.Id &&
                    x.SinifId == id);

            if (atama == null)
            {
                return Unauthorized();
            }

            var sinif = _context.Siniflar
                .FirstOrDefault(x =>
                    x.Id == id);

            if (sinif == null)
            {
                return NotFound();
            }

            var ogrenciler = _context.Ogrenciler
                .Where(x =>
                    x.SinifId.HasValue &&
                    x.SinifId.Value == id)
                .OrderBy(x => x.AdSoyad)
                .ToList();

            var bugun = DateTime.Today;

            var mevcutYoklama = _context.Yoklamalar
                .Include(x => x.Detaylar)
                .FirstOrDefault(x =>
                    x.SinifId == id &&
                    x.OgretmenId == ogretmen.Id &&
                    x.Tarih.Date == bugun);

            ViewBag.Sinif = sinif;
            ViewBag.AlreadyTaken = mevcutYoklama != null;

            if (mevcutYoklama != null)
            {
                ViewBag.MevcutDurumlar = mevcutYoklama.Detaylar
                    .ToDictionary(x => x.OgrenciId, x => x.Geldi);
            }

            return View(ogrenciler);
        }

        // ============================================================
        // ÖĞRETMEN - Yoklamayı kaydet
        // ============================================================

        [HttpPost]
        public IActionResult YoklamaKaydet(
            int sinifId,
            string? aciklama,
            Dictionary<int, bool> durumlar)
        {
            var kullaniciAdi =
                HttpContext.Session.GetString("KullaniciAdi");

            if (string.IsNullOrEmpty(kullaniciAdi))
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var kullanici = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.KullaniciAdi == kullaniciAdi);

            if (kullanici == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x =>
                    x.KullaniciId == kullanici.Id);

            if (ogretmen == null)
            {
                return Unauthorized();
            }

            var atama = _context.OgretmenSiniflar
                .FirstOrDefault(x =>
                    x.OgretmenId == ogretmen.Id &&
                    x.SinifId == sinifId);

            if (atama == null)
            {
                return Unauthorized();
            }

            var ogrenciler = _context.Ogrenciler
                .Where(x =>
                    x.SinifId.HasValue &&
                    x.SinifId.Value == sinifId)
                .ToList();

            if (!ogrenciler.Any())
            {
                return RedirectToAction("Yoklama");
            }

            var bugun = DateTime.Today;

            var mevcutYoklama = _context.Yoklamalar
                .Include(x => x.Detaylar)
                .FirstOrDefault(x =>
                    x.SinifId == sinifId &&
                    x.OgretmenId == ogretmen.Id &&
                    x.Tarih.Date == bugun);

            if (mevcutYoklama != null)
            {
                TempData["YoklamaUyari"] =
                    "Bu sınıfın bugünkü yoklaması daha önce alınmıştır.";

                return RedirectToAction(
                    "YoklamaAl",
                    new { id = sinifId }
                );
            }

            var yoklama = new Yoklama
            {
                SinifId = sinifId,
                OgretmenId = ogretmen.Id,
                Tarih = DateTime.Now,
                Aciklama = string.IsNullOrWhiteSpace(aciklama)
                    ? null
                    : aciklama.Trim()
            };

            _context.Yoklamalar.Add(yoklama);
            _context.SaveChanges();

            foreach (var ogrenci in ogrenciler)
            {
                var geldi = durumlar.ContainsKey(ogrenci.Id)
                    ? durumlar[ogrenci.Id]
                    : false;

                var detay = new YoklamaDetay
                {
                    YoklamaId = yoklama.Id,
                    OgrenciId = ogrenci.Id,
                    Geldi = geldi
                };

                _context.YoklamaDetaylari.Add(detay);
            }

            _context.SaveChanges();

            TempData["YoklamaMesaji"] =
                "Yoklama başarıyla kaydedildi.";

            return RedirectToAction(
                "YoklamaAl",
                new { id = sinifId }
            );
        }
    }
}