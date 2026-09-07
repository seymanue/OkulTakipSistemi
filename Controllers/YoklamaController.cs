using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class YoklamaController : Controller
    {
        private readonly AppDbContext _context;

        public YoklamaController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // YOKLAMA AL
        // ============================================================

        [HttpGet]
        public IActionResult Al(int sinifId)
        {
            var kullaniciId = HttpContext.Session.GetInt32("KullaniciId");

            if (kullaniciId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x => x.KullaniciId == kullaniciId);

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

            var sinif = _context.Siniflar
                .Include(x => x.Okul)
                .FirstOrDefault(x => x.Id == sinifId);

            if (sinif == null)
            {
                return NotFound();
            }

            var ogrenciler = _context.Ogrenciler
                .Where(x => x.SinifId == sinifId)
                .OrderBy(x => x.AdSoyad)
                .ToList();

            ViewBag.Sinif = sinif;

            return View(ogrenciler);
        }


        // ============================================================
        // YOKLAMAYI KAYDET
        // ============================================================

        [HttpPost]
        public IActionResult Kaydet(
            int sinifId,
            Dictionary<int, bool> durumlar)
        {
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("       YOKLAMA KAYDET ÇALIŞTI");
            Console.WriteLine("==============================================");
            Console.WriteLine($"SINIF ID: {sinifId}");
            Console.WriteLine($"ÖĞRENCİ SAYISI: {durumlar?.Count ?? 0}");

            var kullaniciId = HttpContext.Session.GetInt32("KullaniciId");

            if (kullaniciId == null)
            {
                Console.WriteLine("HATA: Kullanıcı oturumu bulunamadı.");
                return RedirectToAction("Login", "Account");
            }

            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x => x.KullaniciId == kullaniciId);

            if (ogretmen == null)
            {
                Console.WriteLine("HATA: Öğretmen bulunamadı.");
                return Unauthorized();
            }

            Console.WriteLine($"ÖĞRETMEN ID: {ogretmen.Id}");

            var atama = _context.OgretmenSiniflar
                .FirstOrDefault(x =>
                    x.OgretmenId == ogretmen.Id &&
                    x.SinifId == sinifId);

            if (atama == null)
            {
                Console.WriteLine("HATA: Öğretmen bu sınıfa atanmamış.");
                return Unauthorized();
            }

            var sinif = _context.Siniflar
                .FirstOrDefault(x => x.Id == sinifId);

            if (sinif == null)
            {
                Console.WriteLine("HATA: Sınıf bulunamadı.");
                return NotFound();
            }

            // --------------------------------------------------------
            // BUGÜN DAHA ÖNCE YOKLAMA ALINMIŞ MI?
            // --------------------------------------------------------

            var bugun = DateTime.Today;
            var yarin = bugun.AddDays(1);

            var mevcutYoklama = _context.Yoklamalar
                .FirstOrDefault(x =>
                    x.SinifId == sinifId &&
                    x.OgretmenId == ogretmen.Id &&
                    x.Tarih >= bugun &&
                    x.Tarih < yarin);

            if (mevcutYoklama != null)
            {
                Console.WriteLine(
                    $"UYARI: Bugün zaten yoklama var. Yoklama ID: {mevcutYoklama.Id}");

                TempData["YoklamaUyari"] =
                    "Bu sınıf için bugün yoklama zaten alınmış. Değişiklik yapmak için Yoklama Geçmişi bölümündeki Düzenle seçeneğini kullanabilirsiniz.";

                return RedirectToAction(
                    "SinifGecmis",
                    new { id = sinifId });
            }

            // --------------------------------------------------------
            // YENİ YOKLAMA
            // --------------------------------------------------------

            var yoklama = new Yoklama
            {
                SinifId = sinifId,
                OgretmenId = ogretmen.Id,
                Tarih = DateTime.Now
            };

            _context.Yoklamalar.Add(yoklama);

            Console.WriteLine("Yoklama nesnesi oluşturuldu.");

            _context.SaveChanges();

            Console.WriteLine($"YOKLAMA KAYDEDİLDİ. ID: {yoklama.Id}");

            // --------------------------------------------------------
            // ÖĞRENCİ DURUMLARINI KAYDET
            // --------------------------------------------------------

            if (durumlar != null)
            {
                foreach (var durum in durumlar)
                {
                    Console.WriteLine(
                        $"ÖĞRENCİ ID: {durum.Key} | GELDİ: {durum.Value}");

                    var ogrenci = _context.Ogrenciler
                        .FirstOrDefault(x =>
                            x.Id == durum.Key &&
                            x.SinifId == sinifId);

                    if (ogrenci == null)
                    {
                        Console.WriteLine(
                            $"UYARI: Öğrenci bulunamadı. ID: {durum.Key}");

                        continue;
                    }

                    var detay = new YoklamaDetay
                    {
                        YoklamaId = yoklama.Id,
                        OgrenciId = durum.Key,
                        Geldi = durum.Value
                    };

                    _context.YoklamaDetaylari.Add(detay);
                }
            }

            _context.SaveChanges();

            Console.WriteLine("YOKLAMA DETAYLARI KAYDEDİLDİ.");
            Console.WriteLine("==============================================");
            Console.WriteLine("       YOKLAMA KAYIT İŞLEMİ TAMAMLANDI");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            TempData["Basari"] =
                "Yoklama başarıyla kaydedildi.";

            return RedirectToAction(
                "SinifGecmis",
                new { id = sinifId });
        }


        // ============================================================
        // YOKLAMA LİSTESİ
        // Öğretmenin atanmış olduğu sınıfları gösterir
        // ============================================================

        [HttpGet]
        public IActionResult Gecmis()
        {
            var kullaniciId = HttpContext.Session.GetInt32("KullaniciId");

            if (kullaniciId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x => x.KullaniciId == kullaniciId);

            if (ogretmen == null)
            {
                return Unauthorized();
            }

            var siniflar = _context.OgretmenSiniflar
                .Where(x => x.OgretmenId == ogretmen.Id)
                .Include(x => x.Sinif)
                .ThenInclude(x => x.Okul)
                .Select(x => x.Sinif)
                .Where(x => x != null)
                .ToList();

            return View(siniflar);
        }


        // ============================================================
        // SEÇİLEN SINIFIN YOKLAMA GEÇMİŞİ
        // ============================================================

        [HttpGet]
        public IActionResult SinifGecmis(int id)
        {
            var kullaniciId = HttpContext.Session.GetInt32("KullaniciId");

            if (kullaniciId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x => x.KullaniciId == kullaniciId);

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
                .Include(x => x.Okul)
                .FirstOrDefault(x => x.Id == id);

            if (sinif == null)
            {
                return NotFound();
            }

            var yoklamalar = _context.Yoklamalar
                .Where(x =>
                    x.SinifId == id &&
                    x.OgretmenId == ogretmen.Id)
                .OrderByDescending(x => x.Tarih)
                .ToList();

            var model = new List<YoklamaGecmisViewModel>();

            foreach (var yoklama in yoklamalar)
            {
                var detaylar = _context.YoklamaDetaylari
                    .Where(d => d.YoklamaId == yoklama.Id)
                    .Join(
                        _context.Ogrenciler,
                        detay => detay.OgrenciId,
                        ogrenci => ogrenci.Id,
                        (detay, ogrenci) => new OgrenciYoklamaViewModel
                        {
                            AdSoyad = ogrenci.AdSoyad,
                            OgrenciNo = ogrenci.OgrenciNo.ToString(),
                            Geldi = detay.Geldi
                        })
                    .OrderBy(x => x.AdSoyad)
                    .ToList();

                model.Add(new YoklamaGecmisViewModel
                {
                    YoklamaId = yoklama.Id,
                    SinifAdi = sinif.Ad,
                    Tarih = yoklama.Tarih,
                    Ogrenciler = detaylar
                });
            }

            ViewBag.Sinif = sinif;

            return View(model);
        }


        // ============================================================
        // YOKLAMA DÜZENLE
        // ============================================================

        [HttpGet]
        public IActionResult Duzenle(int id)
        {
            var kullaniciId = HttpContext.Session.GetInt32("KullaniciId");

            if (kullaniciId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x => x.KullaniciId == kullaniciId);

            if (ogretmen == null)
            {
                return Unauthorized();
            }

            var yoklama = _context.Yoklamalar
                .FirstOrDefault(x =>
                    x.Id == id &&
                    x.OgretmenId == ogretmen.Id);

            if (yoklama == null)
            {
                return NotFound();
            }

            var atama = _context.OgretmenSiniflar
                .FirstOrDefault(x =>
                    x.OgretmenId == ogretmen.Id &&
                    x.SinifId == yoklama.SinifId);

            if (atama == null)
            {
                return Unauthorized();
            }

            var sinif = _context.Siniflar
                .Include(x => x.Okul)
                .FirstOrDefault(x => x.Id == yoklama.SinifId);

            if (sinif == null)
            {
                return NotFound();
            }

            var ogrenciler = _context.Ogrenciler
                .Where(x => x.SinifId == yoklama.SinifId)
                .OrderBy(x => x.AdSoyad)
                .ToList();

            var detaylar = _context.YoklamaDetaylari
                .Where(x => x.YoklamaId == yoklama.Id)
                .ToDictionary(x => x.OgrenciId, x => x.Geldi);

            ViewBag.Sinif = sinif;
            ViewBag.Yoklama = yoklama;
            ViewBag.Detaylar = detaylar;

            return View(ogrenciler);
        }


        // ============================================================
        // YOKLAMA DÜZENLE - KAYDET
        // ============================================================

        [HttpPost]
        public IActionResult DuzenleKaydet(
            int yoklamaId,
            Dictionary<int, bool> durumlar)
        {
            var kullaniciId = HttpContext.Session.GetInt32("KullaniciId");

            if (kullaniciId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x => x.KullaniciId == kullaniciId);

            if (ogretmen == null)
            {
                return Unauthorized();
            }

            var yoklama = _context.Yoklamalar
                .FirstOrDefault(x =>
                    x.Id == yoklamaId &&
                    x.OgretmenId == ogretmen.Id);

            if (yoklama == null)
            {
                return NotFound();
            }

            var atama = _context.OgretmenSiniflar
                .FirstOrDefault(x =>
                    x.OgretmenId == ogretmen.Id &&
                    x.SinifId == yoklama.SinifId);

            if (atama == null)
            {
                return Unauthorized();
            }

            var detaylar = _context.YoklamaDetaylari
                .Where(x => x.YoklamaId == yoklamaId)
                .ToList();

            foreach (var detay in detaylar)
            {
                if (durumlar.ContainsKey(detay.OgrenciId))
                {
                    detay.Geldi = durumlar[detay.OgrenciId];
                }
            }

            _context.SaveChanges();

            TempData["Basari"] =
                "Yoklama başarıyla güncellendi.";

            return RedirectToAction(
                "SinifGecmis",
                new { id = yoklama.SinifId });
        }
    }
}