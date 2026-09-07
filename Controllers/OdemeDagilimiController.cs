using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class OdemeDagilimiController : Controller
    {
        private readonly AppDbContext _context;

        public OdemeDagilimiController(AppDbContext context)
        {
            _context = context;
        }

        // Ödeme dağılımlarını listele
        public IActionResult Index()
        {
            var dagilimlar = _context.OdemeDagilimlari
                .Include(x => x.Odeme)
                .Include(x => x.AylikBorc)
                .ThenInclude(x => x!.OgrenciKaydi)
                .ThenInclude(x => x!.Ogrenci)
                .OrderByDescending(x => x.Id)
                .ToList();

            return View(dagilimlar);
        }

        // Dağılım ekleme sayfası
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Odemeler = _context.Odemeler
                .OrderByDescending(x => x.Id)
                .ToList();

            ViewBag.AylikBorclar = _context.AylikBorclar
                .Include(x => x.OgrenciKaydi)
                .ThenInclude(x => x!.Ogrenci)
                .OrderByDescending(x => x.Yil)
                .ThenByDescending(x => x.Ay)
                .ToList();

            return View();
        }

        // Dağılım ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OdemeDagilimi model)
        {
            if (model.Tutar <= 0)
            {
                ModelState.AddModelError(
                    "Tutar",
                    "Dağıtım tutarı 0'dan büyük olmalıdır."
                );
            }

            var odeme = _context.Odemeler.Find(model.OdemeId);

            if (odeme == null)
            {
                ModelState.AddModelError(
                    "OdemeId",
                    "Geçerli bir ödeme seçilmelidir."
                );
            }

            var borc = _context.AylikBorclar
                .FirstOrDefault(x => x.Id == model.AylikBorcId);

            if (borc == null)
            {
                ModelState.AddModelError(
                    "AylikBorcId",
                    "Geçerli bir aylık borç seçilmelidir."
                );
            }

            if (odeme != null && model.Tutar > odeme.Tutar)
            {
                ModelState.AddModelError(
                    "Tutar",
                    "Dağıtılan tutar ödeme tutarından fazla olamaz."
                );
            }

            if (borc != null && model.Tutar > borc.KalanTutar)
            {
                ModelState.AddModelError(
                    "Tutar",
                    "Dağıtılan tutar borcun kalan tutarından fazla olamaz."
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Odemeler = _context.Odemeler
                    .OrderByDescending(x => x.Id)
                    .ToList();

                ViewBag.AylikBorclar = _context.AylikBorclar
                    .Include(x => x.OgrenciKaydi)
                    .ThenInclude(x => x!.Ogrenci)
                    .OrderByDescending(x => x.Yil)
                    .ThenByDescending(x => x.Ay)
                    .ToList();

                return View(model);
            }

            _context.OdemeDagilimlari.Add(model);

            if (borc != null)
            {
                borc.OdenenTutar += model.Tutar;
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Dağılım sil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var dagilim = _context.OdemeDagilimlari.Find(id);

            if (dagilim == null)
            {
                return NotFound();
            }

            var borc = _context.AylikBorclar
                .FirstOrDefault(x => x.Id == dagilim.AylikBorcId);

            if (borc != null)
            {
                borc.OdenenTutar -= dagilim.Tutar;

                if (borc.OdenenTutar < 0)
                {
                    borc.OdenenTutar = 0;
                }
            }

            _context.OdemeDagilimlari.Remove(dagilim);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
