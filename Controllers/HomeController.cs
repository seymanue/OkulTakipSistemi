using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // ANA SAYFA
        // =========================

        public IActionResult Index()
        {
            return View();
        }

        // =========================
        // YÖNETİCİ PANELİ
        // =========================

        public IActionResult AdminPanel()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // =========================
        // ÖĞRETMEN PANELİ
        // =========================

        public IActionResult OgretmenPanel()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Öğretmen")
            {
                return RedirectToAction("Login", "Account");
            }

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
                .Select(x => x.SinifId)
                .ToList();

            var sinifSayisi = siniflar.Count;

            var ogrenciSayisi = _context.Ogrenciler
                .Count(x =>
                    x.SinifId.HasValue &&
                    siniflar.Contains(x.SinifId.Value) &&
                    x.Aktif);

            var bugunkuYoklamaSayisi = _context.Yoklamalar
                .Count(x =>
                    x.OgretmenId == ogretmen.Id &&
                    x.Tarih.Date == DateTime.Today);

            var toplamYoklamaSayisi = _context.Yoklamalar
                .Count(x => x.OgretmenId == ogretmen.Id);

            ViewBag.Ogretmen = ogretmen;
            ViewBag.SinifSayisi = sinifSayisi;
            ViewBag.OgrenciSayisi = ogrenciSayisi;
            ViewBag.BugunkuYoklamaSayisi = bugunkuYoklamaSayisi;
            ViewBag.ToplamYoklamaSayisi = toplamYoklamaSayisi;

            return View();
        }

        // =========================
        // GİZLİLİK
        // =========================

        public IActionResult Privacy()
        {
            return View();
        }

        // =========================
        // HATA SAYFASI
        // =========================

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}