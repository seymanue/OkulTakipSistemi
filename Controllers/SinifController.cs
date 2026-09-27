using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class SinifController : Controller
    {
        private readonly AppDbContext _context;

        public SinifController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // SINIFLARI LİSTELE
        // =========================
        public IActionResult Index()
        {
            var siniflar = _context.Siniflar
                .Include(x => x.Okul)
                .OrderBy(x => x.OkulId)
                .ThenBy(x => x.Ad)
                .ToList();

            return View(siniflar);
        }

        // =========================
        // YENİ SINIF - GET
        // =========================
        [HttpGet]
        public IActionResult Create(int? okulId)
        {
            ViewBag.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            ViewBag.Siniflar = _context.Siniflar
                .OrderBy(x => x.Ad)
                .ToList();

            if (okulId.HasValue)
            {
                var okul = _context.Okullar
                    .FirstOrDefault(x => x.Id == okulId.Value);

                if (okul != null)
                {
                    var sinif = new Sinif
                    {
                        OkulId = okul.Id,
                        Aktif = true
                    };

                    return View(sinif);
                }
            }

            return View();
        }

        // =========================
        // YENİ SINIF - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Sinif sinif)
        {
            if (sinif.OkulId == 0)
            {
                ModelState.AddModelError("OkulId", "Okul seçmelisiniz.");
            }

            if (string.IsNullOrWhiteSpace(sinif.Ad))
            {
                ModelState.AddModelError("Ad", "Sınıf adı boş bırakılamaz.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Okullar = _context.Okullar
                    .OrderBy(x => x.Ad)
                    .ToList();

                return View(sinif);
            }

            _context.Siniflar.Add(sinif);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // SINIF DÜZENLE - GET
        // =========================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var sinif = _context.Siniflar
                .FirstOrDefault(x => x.Id == id);

            if (sinif == null)
            {
                return NotFound();
            }

            ViewBag.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            return View(sinif);
        }

        // =========================
        // SINIF DÜZENLE - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Sinif model)
        {
            if (model.OkulId == 0)
            {
                ModelState.AddModelError("OkulId", "Okul seçmelisiniz.");
            }

            if (string.IsNullOrWhiteSpace(model.Ad))
            {
                ModelState.AddModelError("Ad", "Sınıf adı boş bırakılamaz.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Okullar = _context.Okullar
                    .OrderBy(x => x.Ad)
                    .ToList();

                return View(model);
            }

            var sinif = _context.Siniflar
                .FirstOrDefault(x => x.Id == model.Id);

            if (sinif == null)
            {
                return NotFound();
            }

            // Sadece değiştirilmesi gereken alanları güncelle
            sinif.Ad = model.Ad;
            sinif.OkulId = model.OkulId;
            sinif.Aktif = model.Aktif;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // SİL
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var sinif = _context.Siniflar
                .FirstOrDefault(x => x.Id == id);

            if (sinif == null)
                return NotFound();

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 1. Bu sınıfa ait yoklama detaylarını sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM YoklamaDetaylari
                    WHERE YoklamaId IN (
                        SELECT Id
                        FROM Yoklamalar
                        WHERE SinifId = {id}
                    )
                    """);

                // 2. Bu sınıfa ait yoklamaları sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM Yoklamalar
                    WHERE SinifId = {id}
                    """);

                // 3. Öğretmen-sınıf bağlantılarını sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM OgretmenSiniflar
                    WHERE SinifId = {id}
                    """);

                // 4. Bu sınıfa ait öğrenci kayıtlarını al
                var kayitIds = _context.OgrenciKayitlari
                    .Where(x => x.SinifId == id)
                    .Select(x => x.Id)
                    .ToList();

                // 5. Öğrenci kayıtlarına bağlı ücret/borç kayıtlarını sil
                foreach (var kayitId in kayitIds)
                {
                    _context.Database.ExecuteSqlInterpolated($"""
                        DELETE FROM OdemeDagilimlari
                        WHERE AylikBorcId IN (
                            SELECT Id
                            FROM AylikBorclar
                            WHERE OgrenciKaydiId = {kayitId}
                        )
                        """);

                    _context.Database.ExecuteSqlInterpolated($"""
                        DELETE FROM OgrenciOzelUcretler
                        WHERE OgrenciKaydiId = {kayitId}
                        """);

                    _context.Database.ExecuteSqlInterpolated($"""
                        DELETE FROM AylikBorclar
                        WHERE OgrenciKaydiId = {kayitId}
                        """);

                    _context.Database.ExecuteSqlInterpolated($"""
                        DELETE FROM OgrenciUcretDurumlari
                        WHERE OgrenciKaydiId = {kayitId}
                        """);
                }

                // 6. Öğrenci kayıtlarını sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM OgrenciKayitlari
                    WHERE SinifId = {id}
                    """);

                // 7. Sınıfa bağlı öğrencilerin SinifId alanını boşalt
                _context.Database.ExecuteSqlInterpolated($"""
                    UPDATE Ogrenciler
                    SET SinifId = NULL
                    WHERE SinifId = {id}
                    """);

                // 8. Sınıfı sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM Siniflar
                    WHERE Id = {id}
                    """);

                transaction.Commit();

                TempData["Basarili"] =
                    sinif.Ad + " sınıfı ve bağlı kayıtları başarıyla silindi.";
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                Console.WriteLine("================================");
                Console.WriteLine("SINIF SİLME HATASI");
                Console.WriteLine("SINIF ID: " + id);
                Console.WriteLine("HATA: " + ex.Message);
                Console.WriteLine("================================");

                TempData["Hata"] =
                    sinif.Ad + " sınıfı silinemedi. İşlem geri alındı.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}