using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class OgretmenSinifController : Controller
    {
        private readonly AppDbContext _context;

        public OgretmenSinifController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================
        // ATAMALARI LİSTELE
        // ============================================
        public IActionResult Index()
        {
            var atamalar = _context.OgretmenSiniflar
                .Include(x => x.Ogretmen)
                    .ThenInclude(x => x.Kullanici)
                .Include(x => x.Sinif)
                    .ThenInclude(x => x.Okul)
                .ToList();

            return View(atamalar);
        }

        // ============================================
        // ATAMA EKRANI
        // ============================================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Ogretmenler = _context.Ogretmenler
                .Include(x => x.Kullanici)
                .ToList();

            // Sadece aktif sınıfları getir
            ViewBag.Siniflar = _context.Siniflar
                .Include(x => x.Okul)
                .Where(x => x.Aktif)
                .OrderBy(x => x.Okul.Ad)
                .ThenBy(x => x.Ad)
                .ToList();

            return View();
        }

        // ============================================
        // ATAMA KAYDET
        // ============================================
        [HttpPost]
        public IActionResult Create(OgretmenSinif model)
        {
            ViewBag.Ogretmenler = _context.Ogretmenler
                .Include(x => x.Kullanici)
                .ToList();

            ViewBag.Siniflar = _context.Siniflar
                .Include(x => x.Okul)
                .Where(x => x.Aktif)
                .OrderBy(x => x.Okul.Ad)
                .ThenBy(x => x.Ad)
                .ToList();

            if (model.OgretmenId == 0 || model.SinifId == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Öğretmen ve sınıf seçmelisiniz."
                );

                return View(model);
            }

            // ============================================
            // SINIFI BUL
            // ============================================
            var sinif = _context.Siniflar
                .Include(x => x.Okul)
                .FirstOrDefault(x => x.Id == model.SinifId);

            if (sinif == null)
            {
                ModelState.AddModelError(
                    "",
                    "Seçilen sınıf bulunamadı."
                );

                return View(model);
            }

            // Pasif sınıfa öğretmen atanmasını engelle
            if (!sinif.Aktif)
            {
                ModelState.AddModelError(
                    "",
                    "Pasif bir sınıfa öğretmen atanamaz."
                );

                return View(model);
            }

            // ============================================
            // ÖĞRETMENİ BUL
            // ============================================
            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x => x.Id == model.OgretmenId);

            if (ogretmen == null)
            {
                ModelState.AddModelError(
                    "",
                    "Seçilen öğretmen bulunamadı."
                );

                return View(model);
            }

            // ============================================
            // AYNI ATAMA VAR MI?
            // ============================================
            var mevcutAtama = _context.OgretmenSiniflar
                .FirstOrDefault(x =>
                    x.OgretmenId == model.OgretmenId &&
                    x.SinifId == model.SinifId
                );

            if (mevcutAtama != null)
            {
                ModelState.AddModelError(
                    "",
                    "Bu öğretmen zaten bu sınıfa atanmış."
                );

                return View(model);
            }

            // ============================================
            // KAYDET
            // ============================================
            _context.OgretmenSiniflar.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ============================================
        // DÜZENLEME EKRANI
        // ============================================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var atama = _context.OgretmenSiniflar
                .FirstOrDefault(x => x.Id == id);

            if (atama == null)
            {
                return NotFound();
            }

            ViewBag.Ogretmenler = _context.Ogretmenler
                .Include(x => x.Kullanici)
                .ToList();

            // Aktif sınıflar + mevcut atanmış sınıf
            ViewBag.Siniflar = _context.Siniflar
                .Include(x => x.Okul)
                .Where(x => x.Aktif || x.Id == atama.SinifId)
                .OrderBy(x => x.Okul.Ad)
                .ThenBy(x => x.Ad)
                .ToList();

            return View(atama);
        }

        // ============================================
        // DÜZENLEMEYİ KAYDET
        // ============================================
        [HttpPost]
        public IActionResult Edit(OgretmenSinif model)
        {
            ViewBag.Ogretmenler = _context.Ogretmenler
                .Include(x => x.Kullanici)
                .ToList();

            var mevcutSiniflar = _context.Siniflar
                .Include(x => x.Okul)
                .Where(x => x.Aktif || x.Id == model.SinifId)
                .OrderBy(x => x.Okul.Ad)
                .ThenBy(x => x.Ad)
                .ToList();

            ViewBag.Siniflar = mevcutSiniflar;

            if (model.OgretmenId == 0 || model.SinifId == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Öğretmen ve sınıf seçmelisiniz."
                );

                return View(model);
            }

            // ============================================
            // SINIFI BUL
            // ============================================
            var sinif = _context.Siniflar
                .Include(x => x.Okul)
                .FirstOrDefault(x => x.Id == model.SinifId);

            if (sinif == null)
            {
                ModelState.AddModelError(
                    "",
                    "Seçilen sınıf bulunamadı."
                );

                return View(model);
            }

            // Pasif sınıfa atama yapılmasını engelle
            if (!sinif.Aktif)
            {
                ModelState.AddModelError(
                    "",
                    "Pasif bir sınıfa öğretmen atanamaz."
                );

                return View(model);
            }

            // ============================================
            // AYNI ATAMA VAR MI?
            // ============================================
            var mevcutAtama = _context.OgretmenSiniflar
                .FirstOrDefault(x =>
                    x.OgretmenId == model.OgretmenId &&
                    x.SinifId == model.SinifId &&
                    x.Id != model.Id
                );

            if (mevcutAtama != null)
            {
                ModelState.AddModelError(
                    "",
                    "Bu öğretmen zaten bu sınıfa atanmış."
                );

                return View(model);
            }

            // ============================================
            // MEVCUT ATAMAYI BUL
            // ============================================
            var atama = _context.OgretmenSiniflar
                .FirstOrDefault(x => x.Id == model.Id);

            if (atama == null)
            {
                return NotFound();
            }

            // ============================================
            // GÜNCELLE
            // ============================================
            atama.OgretmenId = model.OgretmenId;
            atama.SinifId = model.SinifId;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ============================================
        // ATAMA SİL
        // ============================================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var atama = _context.OgretmenSiniflar
                .FirstOrDefault(x => x.Id == id);

            if (atama == null)
            {
                return NotFound();
            }

            _context.OgretmenSiniflar.Remove(atama);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}