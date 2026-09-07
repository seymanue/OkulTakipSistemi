using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class OgrenciKaydiController : Controller
    {
        private readonly AppDbContext _context;

        public OgrenciKaydiController(AppDbContext context)
        {
            _context = context;
        }

        // Kayıtları listele
        public IActionResult Index()
        {
            var kayitlar = _context.OgrenciKayitlari
                .Include(x => x.Ogrenci)
                .Include(x => x.Okul)
                .Include(x => x.Sinif)
                .OrderBy(x => x.Ogrenci!.AdSoyad)
                .ToList();

            return View(kayitlar);
        }

        // Yeni kayıt sayfası
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Ogrenciler = _context.Ogrenciler
                .OrderBy(x => x.AdSoyad)
                .ToList();

            ViewBag.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            ViewBag.Siniflar = _context.Siniflar
                .Include(x => x.Okul)
                .OrderBy(x => x.Ad)
                .ToList();

            return View();
        }

        // Yeni kayıt oluştur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OgrenciKaydi kayit)
        {
            if (kayit.OgrenciId <= 0)
            {
                ModelState.AddModelError("OgrenciId", "Öğrenci seçiniz.");
            }

            if (kayit.OkulId <= 0)
            {
                ModelState.AddModelError("OkulId", "Okul seçiniz.");
            }

            if (kayit.SinifId <= 0)
            {
                ModelState.AddModelError("SinifId", "Sınıf seçiniz.");
            }

            var sinif = _context.Siniflar
                .FirstOrDefault(x => x.Id == kayit.SinifId);

            if (sinif != null && sinif.OkulId != kayit.OkulId)
            {
                ModelState.AddModelError(
                    "SinifId",
                    "Seçilen sınıf, seçilen okula ait değil."
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Ogrenciler = _context.Ogrenciler
                    .OrderBy(x => x.AdSoyad)
                    .ToList();

                ViewBag.Okullar = _context.Okullar
                    .OrderBy(x => x.Ad)
                    .ToList();

                ViewBag.Siniflar = _context.Siniflar
                    .Include(x => x.Okul)
                    .OrderBy(x => x.Ad)
                    .ToList();

                return View(kayit);
            }

            _context.OgrenciKayitlari.Add(kayit);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Düzenleme sayfası
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var kayit = _context.OgrenciKayitlari
                .FirstOrDefault(x => x.Id == id);

            if (kayit == null)
            {
                return NotFound();
            }

            ViewBag.Ogrenciler = _context.Ogrenciler
                .OrderBy(x => x.AdSoyad)
                .ToList();

            ViewBag.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            ViewBag.Siniflar = _context.Siniflar
                .Include(x => x.Okul)
                .OrderBy(x => x.Ad)
                .ToList();

            return View(kayit);
        }

        // Düzenleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(OgrenciKaydi kayit)
        {
            var sinif = _context.Siniflar
                .FirstOrDefault(x => x.Id == kayit.SinifId);

            if (kayit.OgrenciId <= 0)
            {
                ModelState.AddModelError("OgrenciId", "Öğrenci seçiniz.");
            }

            if (kayit.OkulId <= 0)
            {
                ModelState.AddModelError("OkulId", "Okul seçiniz.");
            }

            if (kayit.SinifId <= 0)
            {
                ModelState.AddModelError("SinifId", "Sınıf seçiniz.");
            }

            if (sinif != null && sinif.OkulId != kayit.OkulId)
            {
                ModelState.AddModelError(
                    "SinifId",
                    "Seçilen sınıf, seçilen okula ait değil."
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Ogrenciler = _context.Ogrenciler
                    .OrderBy(x => x.AdSoyad)
                    .ToList();

                ViewBag.Okullar = _context.Okullar
                    .OrderBy(x => x.Ad)
                    .ToList();

                ViewBag.Siniflar = _context.Siniflar
                    .Include(x => x.Okul)
                    .OrderBy(x => x.Ad)
                    .ToList();

                return View(kayit);
            }

            var mevcutKayit = _context.OgrenciKayitlari
                .FirstOrDefault(x => x.Id == kayit.Id);

            if (mevcutKayit == null)
            {
                return NotFound();
            }

            mevcutKayit.OgrenciId = kayit.OgrenciId;
            mevcutKayit.OkulId = kayit.OkulId;
            mevcutKayit.SinifId = kayit.SinifId;
            mevcutKayit.Aktif = kayit.Aktif;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Kayıt sil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var kayit = _context.OgrenciKayitlari
                .FirstOrDefault(x => x.Id == id);

            if (kayit == null)
            {
                return NotFound();
            }

            _context.OgrenciKayitlari.Remove(kayit);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
