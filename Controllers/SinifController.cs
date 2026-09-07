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
        public IActionResult Create()
        {
            ViewBag.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

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

            // Yeni oluşturulan sınıf aktif olsun
            sinif.Aktif = true;

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
        // AKTİF / PASİF DEĞİŞTİR
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AktifPasif(int id)
        {
            var sinif = _context.Siniflar
                .FirstOrDefault(x => x.Id == id);

            if (sinif == null)
            {
                return NotFound();
            }

            sinif.Aktif = !sinif.Aktif;

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
            {
                return NotFound();
            }

            _context.Siniflar.Remove(sinif);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}