using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class OkulUcretiController : Controller
    {
        private readonly AppDbContext _context;

        public OkulUcretiController(AppDbContext context)
        {
            _context = context;
        }

        // OKUL ÜCRETLERİ
        [HttpGet]
        public IActionResult Index()
        {
            var ucretler = _context.OkulUcretleri
                .Include(x => x.Okul)
                .OrderBy(x => x.Okul.Ad)
                .ThenByDescending(x => x.Id)
                .ToList();

            return View(ucretler);
        }

        // YENİ OKUL + ÜCRET EKLEME
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // YENİ OKUL + ÜCRET + SINIFLARI KAYDET
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            OkulUcreti okulUcreti,
            string okulAdi,
            string adres,
            string telefon,
            List<string>? sinifAdlari)
        {
            if (string.IsNullOrWhiteSpace(okulAdi))
            {
                ModelState.AddModelError("okulAdi", "Okul adı zorunludur.");
            }

            if (okulUcreti.ToplamUcret <= 0)
            {
                ModelState.AddModelError(
                    nameof(okulUcreti.ToplamUcret),
                    "Yıllık ücret 0'dan büyük olmalıdır.");
            }

            // Boş sınıf isimlerini temizle ve aynı sınıfı iki kez eklemeyi engelle.
            var temizSiniflar = (sinifAdlari ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!ModelState.IsValid)
            {
                return View(okulUcreti);
            }

            // YENİ OKUL
            var okul = new Okul
            {
                Ad = okulAdi.Trim(),
                Adres = adres?.Trim() ?? "",
                Telefon = telefon?.Trim() ?? ""
            };

            _context.Okullar.Add(okul);

            // YILLIK ÜCRET
            okulUcreti.Okul = okul;
            okulUcreti.OlusturmaTarihi = DateTime.Now;
            okulUcreti.GecerlilikTarihi =
                new DateTime(DateTime.Now.Year, 1, 1);

            okulUcreti.StandartVeliUcreti = okulUcreti.ToplamUcret;
            okulUcreti.OdemeSekli = "Taksitli";
            okulUcreti.TaksitAySayisi = 12;
            okulUcreti.BaslangicAyi = 9;

            _context.OkulUcretleri.Add(okulUcreti);

            // SINIFLAR
            foreach (var sinifAdi in temizSiniflar)
            {
                _context.Siniflar.Add(new Sinif
                {
                    Ad = sinifAdi,
                    Okul = okul,
                    Aktif = true
                });
            }

            // Okul + ücret + sınıflar birlikte kaydedilir.
            _context.SaveChanges();

            TempData["Basarili"] =
                okul.Ad + " okulu, yıllık ücreti" +
                (temizSiniflar.Count > 0
                    ? " ve " + temizSiniflar.Count + " sınıfı"
                    : "") +
                " başarıyla kaydedildi.";

            return RedirectToAction(nameof(Index));
        }

        // OKUL + ÜCRET DÜZENLEME
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var okulUcreti = _context.OkulUcretleri
                .Include(x => x.Okul)
                .FirstOrDefault(x => x.Id == id);

            if (okulUcreti == null || okulUcreti.Okul == null)
            {
                return NotFound();
            }

            ViewBag.Siniflar = _context.Siniflar
                .Where(x => x.OkulId == okulUcreti.OkulId)
                .OrderBy(x => x.Ad)
                .ToList();

            return View(okulUcreti);
        }

        // OKUL + ÜCRET + SINIFLARI GÜNCELLE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            OkulUcreti model,
            string okulAdi,
            string adres,
            string telefon,
            List<int>? silinecekSinifIds,
            List<string>? yeniSinifAdlari)
        {
            var okulUcreti = _context.OkulUcretleri
                .Include(x => x.Okul)
                .FirstOrDefault(x => x.Id == id);

            if (okulUcreti == null || okulUcreti.Okul == null)
            {
                return NotFound();
            }

            var okul = okulUcreti.Okul;

            if (string.IsNullOrWhiteSpace(okulAdi))
            {
                ModelState.AddModelError(
                    "okulAdi",
                    "Okul adı zorunludur.");
            }

            if (model.ToplamUcret <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.ToplamUcret),
                    "Yıllık ücret 0'dan büyük olmalıdır.");
            }

            var temizYeniSiniflar = (yeniSinifAdlari ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!ModelState.IsValid)
            {
                model.Id = id;
                model.OkulId = okul.Id;
                model.Okul = okul;

                ViewBag.Siniflar = _context.Siniflar
                    .Where(x => x.OkulId == okul.Id)
                    .OrderBy(x => x.Ad)
                    .ToList();

                return View(model);
            }

            // OKUL BİLGİLERİ
            okul.Ad = okulAdi.Trim();
            okul.Adres = adres?.Trim() ?? "";
            okul.Telefon = telefon?.Trim() ?? "";

            // YILLIK ÜCRET
            okulUcreti.ToplamUcret = model.ToplamUcret;
            okulUcreti.StandartVeliUcreti = model.ToplamUcret;
            okulUcreti.Aciklama = model.Aciklama;

            // SINIF SİLME
            if (silinecekSinifIds != null && silinecekSinifIds.Count > 0)
            {
                using var transaction = _context.Database.BeginTransaction();

                try
                {
                    var silinecekSiniflar = _context.Siniflar
                        .Where(x =>
                            x.OkulId == okul.Id &&
                            silinecekSinifIds.Contains(x.Id))
                        .ToList();

                    foreach (var sinif in silinecekSiniflar)
                    {
                        var sinifId = sinif.Id;

                        // 1. Yoklama detaylarını sil
                        _context.Database.ExecuteSqlInterpolated($"""
                            DELETE FROM YoklamaDetaylari
                            WHERE YoklamaId IN (
                                SELECT Id
                                FROM Yoklamalar
                                WHERE SinifId = {sinifId}
                            )
                            """);

                        // 2. Yoklamaları sil
                        _context.Database.ExecuteSqlInterpolated($"""
                            DELETE FROM Yoklamalar
                            WHERE SinifId = {sinifId}
                            """);

                        // 3. Öğretmen-sınıf bağlantılarını sil
                        _context.Database.ExecuteSqlInterpolated($"""
                            DELETE FROM OgretmenSiniflar
                            WHERE SinifId = {sinifId}
                            """);

                        // 4. Bu sınıfa ait öğrenci kayıtlarını bul
                        var kayitIds = _context.OgrenciKayitlari
                            .Where(x => x.SinifId == sinifId)
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
                            WHERE SinifId = {sinifId}
                            """);

                        // 7. Öğrencilerin sınıf bağlantısını kaldır
                        _context.Database.ExecuteSqlInterpolated($"""
                            UPDATE Ogrenciler
                            SET SinifId = NULL
                            WHERE SinifId = {sinifId}
                            """);

                        // 8. Sınıfı sil
                        _context.Database.ExecuteSqlInterpolated($"""
                            DELETE FROM Siniflar
                            WHERE Id = {sinifId}
                            """);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            // YENİ SINIF EKLEME
            foreach (var sinifAdi in temizYeniSiniflar)
            {
                var zatenVar = _context.Siniflar.Any(x =>
                    x.OkulId == okul.Id &&
                    x.Ad.ToLower() == sinifAdi.ToLower());

                if (!zatenVar)
                {
                    _context.Siniflar.Add(new Sinif
                    {
                        Ad = sinifAdi,
                        OkulId = okul.Id,
                        Aktif = true
                    });
                }
            }

            _context.SaveChanges();

            TempData["Basarili"] =
                okul.Ad + " okulunun bilgileri başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }

        // ÜCRET SİL
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var ucret = _context.OkulUcretleri
                .FirstOrDefault(x => x.Id == id);

            if (ucret == null)
                return NotFound();

            _context.OkulUcretleri.Remove(ucret);
            _context.SaveChanges();

            TempData["Basarili"] =
                "Okul ücreti başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // OKULU TAMAMEN SİL
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSchool(int id)
        {
            var okul = _context.Okullar
                .FirstOrDefault(x => x.Id == id);

            if (okul == null)
                return NotFound();

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 1. Okula ait sınıfları al
                var sinifIds = _context.Siniflar
                    .Where(x => x.OkulId == id)
                    .Select(x => x.Id)
                    .ToList();

                // 2. Okula ait öğrenci kayıtlarını al
                var kayitIds = _context.OgrenciKayitlari
                    .Where(x => x.OkulId == id)
                    .Select(x => x.Id)
                    .ToList();

                // 3. Bu kayıtların bağlı olduğu öğrencileri al
                var ogrenciIds = _context.OgrenciKayitlari
                    .Where(x => x.OkulId == id)
                    .Select(x => x.OgrenciId)
                    .Distinct()
                    .ToList();

                // 4. Okula ait sınıfların yoklama detaylarını sil
                foreach (var sinifId in sinifIds)
                {
                    _context.Database.ExecuteSqlInterpolated($"""
                        DELETE FROM YoklamaDetaylari
                        WHERE YoklamaId IN (
                            SELECT Id
                            FROM Yoklamalar
                            WHERE SinifId = {sinifId}
                        )
                        """);
                }

                // 5. Okula ait yoklamaları sil
                foreach (var sinifId in sinifIds)
                {
                    _context.Database.ExecuteSqlInterpolated($"""
                        DELETE FROM Yoklamalar
                        WHERE SinifId = {sinifId}
                        """);
                }

                // 6. Öğretmen-sınıf bağlantılarını sil
                foreach (var sinifId in sinifIds)
                {
                    _context.Database.ExecuteSqlInterpolated($"""
                        DELETE FROM OgretmenSiniflar
                        WHERE SinifId = {sinifId}
                        """);
                }

                // 7. Öğrenci kayıtlarına bağlı ödeme dağılımlarını sil
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

                // 8. Okula ait öğrenci kayıtlarını sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM OgrenciKayitlari
                    WHERE OkulId = {id}
                    """);

                // 9. Okula ait ücretleri sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM OkulUcretleri
                    WHERE OkulId = {id}
                    """);

                // 10. Okulun sınıflarına bağlı öğrencilerin sınıf bağlantısını kaldır
                foreach (var sinifId in sinifIds)
                {
                    _context.Database.ExecuteSqlInterpolated($"""
                        UPDATE Ogrenciler
                        SET SinifId = NULL
                        WHERE SinifId = {sinifId}
                        """);
                }

                // 11. Okula ait sınıfları sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM Siniflar
                    WHERE OkulId = {id}
                    """);

                // 12. Artık başka okul kaydı olmayan öğrencileri belirle
                var silinecekOgrenciIds = ogrenciIds
                    .Where(ogrenciId =>
                        !_context.OgrenciKayitlari
                            .Any(x => x.OgrenciId == ogrenciId) &&
                        !_context.Ogrenciler
                            .Where(x => x.Id == ogrenciId)
                            .Select(x => x.SinifId)
                            .Any(sinifId => sinifId != null))
                    .ToList();

                // 13. Silinecek öğrencilerin ödeme dağılımlarını sil
                foreach (var ogrenciId in silinecekOgrenciIds)
                {
                    _context.Database.ExecuteSqlInterpolated($"""
                        DELETE FROM OdemeDagilimlari
                        WHERE OdemeId IN (
                            SELECT Id
                            FROM Odemeler
                            WHERE OgrenciId = {ogrenciId}
                        )
                        """);
                }

                // 14. Silinecek öğrencilerin ödemelerini sil
                foreach (var ogrenciId in silinecekOgrenciIds)
                {
                    _context.Database.ExecuteSqlInterpolated($"""
                        DELETE FROM Odemeler
                        WHERE OgrenciId = {ogrenciId}
                        """);
                }

                // 15. Öğrenci-veli bağlantılarını sil
                foreach (var ogrenciId in silinecekOgrenciIds)
                {
                    _context.Database.ExecuteSqlInterpolated($"""
                        DELETE FROM OgrenciVeliler
                        WHERE OgrenciId = {ogrenciId}
                        """);
                }

                // 16. Ana öğrenci kayıtlarını sil
                foreach (var ogrenciId in silinecekOgrenciIds)
                {
                    _context.Database.ExecuteSqlInterpolated($"""
                        DELETE FROM Ogrenciler
                        WHERE Id = {ogrenciId}
                        """);
                }

                // 17. En son okulu sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM Okullar
                    WHERE Id = {id}
                    """);

                transaction.Commit();

                TempData["Basarili"] =
                    okul.Ad + " okulu ve bağlı kayıtları başarıyla silindi.";
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                Console.WriteLine("================================");
                Console.WriteLine("OKUL SİLME HATASI");
                Console.WriteLine("OKUL ID: " + id);
                Console.WriteLine("HATA: " + ex.Message);
                Console.WriteLine("================================");

                TempData["Hata"] =
                    okul.Ad + " okulu silinemedi. İşlem geri alındı.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
