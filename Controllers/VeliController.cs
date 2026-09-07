using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class VeliController : Controller
    {
        private readonly AppDbContext _context;

        public VeliController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var veliler = _context.Veliler
                .Include(v => v.OgrenciVeliler)
                .ThenInclude(ov => ov.Ogrenci)
                .OrderBy(v => v.AdSoyad)
                .ToList();

            return View(veliler);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Ogrenciler = _context.Ogrenciler
                .OrderBy(x => x.AdSoyad)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Veli veli, int OgrenciId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Ogrenciler = _context.Ogrenciler
                    .OrderBy(x => x.AdSoyad)
                    .ToList();

                return View(veli);
            }

            _context.Veliler.Add(veli);
            _context.SaveChanges();

            if (OgrenciId > 0)
            {
                var ogrenciVeli = new OgrenciVeli
                {
                    OgrenciId = OgrenciId,
                    VeliId = veli.Id
                };

                _context.OgrenciVeliler.Add(ogrenciVeli);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        // DÜZENLEME SAYFASI
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var veli = _context.Veliler
                .Include(v => v.OgrenciVeliler)
                .FirstOrDefault(v => v.Id == id);

            if (veli == null)
            {
                return NotFound();
            }

            ViewBag.Ogrenciler = _context.Ogrenciler
                .OrderBy(x => x.AdSoyad)
                .ToList();

            var mevcutBaglanti = veli.OgrenciVeliler.FirstOrDefault();

            ViewBag.MevcutOgrenciId =
                mevcutBaglanti?.OgrenciId ?? 0;

            return View(veli);
        }

        // DÜZENLEME KAYDET
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Veli veli, int OgrenciId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Ogrenciler = _context.Ogrenciler
                    .OrderBy(x => x.AdSoyad)
                    .ToList();

                ViewBag.MevcutOgrenciId = OgrenciId;

                return View(veli);
            }

            var mevcutVeli = _context.Veliler
                .Include(v => v.OgrenciVeliler)
                .FirstOrDefault(v => v.Id == veli.Id);

            if (mevcutVeli == null)
            {
                return NotFound();
            }

            mevcutVeli.AdSoyad = veli.AdSoyad;
            mevcutVeli.Telefon = veli.Telefon;
            mevcutVeli.Yakinlik = veli.Yakinlik;

            // Eski öğrenci bağlantısını kaldır
            _context.OgrenciVeliler.RemoveRange(
                mevcutVeli.OgrenciVeliler
            );

            // Yeni öğrenci bağlantısını oluştur
            if (OgrenciId > 0)
            {
                _context.OgrenciVeliler.Add(new OgrenciVeli
                {
                    OgrenciId = OgrenciId,
                    VeliId = mevcutVeli.Id
                });
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // SİL
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var veli = _context.Veliler
                .Include(v => v.OgrenciVeliler)
                .FirstOrDefault(v => v.Id == id);

            if (veli == null)
            {
                return NotFound();
            }

            _context.OgrenciVeliler.RemoveRange(
                veli.OgrenciVeliler
            );

            _context.Veliler.Remove(veli);

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
