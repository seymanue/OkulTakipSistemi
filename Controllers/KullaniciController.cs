using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class KullaniciController : Controller
    {
        private readonly AppDbContext _context;

        public KullaniciController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // KULLANICILARI LİSTELE
        // ============================================================

        public IActionResult Index()
        {
            var kullanicilar = _context.Kullanicilar
                .Include(x => x.Rol)
                .ToList();

            return View(kullanicilar);
        }

        // ============================================================
        // KULLANICI EKLEME SAYFASI
        // ============================================================

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roller = _context.Roller.ToList();

            return View();
        }

        // ============================================================
        // KULLANICI EKLEME
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Kullanici kullanici)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roller = _context.Roller.ToList();

                return View(kullanici);
            }

            // Aynı kullanıcı adı var mı?
            var mevcutKullanici = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.KullaniciAdi == kullanici.KullaniciAdi);

            if (mevcutKullanici != null)
            {
                ModelState.AddModelError(
                    "KullaniciAdi",
                    "Bu kullanıcı adı zaten kullanılıyor."
                );

                ViewBag.Roller = _context.Roller.ToList();

                return View(kullanici);
            }

            // Seçilen rol gerçekten var mı?
            var rol = _context.Roller
                .FirstOrDefault(x =>
                    x.Id == kullanici.RolId);

            if (rol == null)
            {
                ModelState.AddModelError(
                    "RolId",
                    "Geçerli bir rol seçmelisiniz."
                );

                ViewBag.Roller = _context.Roller.ToList();

                return View(kullanici);
            }

            _context.Kullanicilar.Add(kullanici);

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // KULLANICI SİLME
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var kullanici = _context.Kullanicilar
                .FirstOrDefault(x => x.Id == id);

            if (kullanici == null)
            {
                return NotFound();
            }

            // Bu kullanıcı bir öğretmene bağlı mı?
            var ogretmen = _context.Ogretmenler
                .FirstOrDefault(x =>
                    x.KullaniciId == id);

            if (ogretmen != null)
            {
                TempData["Hata"] =
                    "Bu kullanıcı bir öğretmene bağlı olduğu için silinemez.";

                return RedirectToAction(nameof(Index));
            }

            // Ana admin silinmesin
            if (kullanici.KullaniciAdi == "admin")
            {
                TempData["Hata"] =
                    "Ana admin kullanıcısı silinemez.";

                return RedirectToAction(nameof(Index));
            }

            _context.Kullanicilar.Remove(kullanici);

            _context.SaveChanges();

            TempData["Basari"] =
                "Kullanıcı başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }
    }
}