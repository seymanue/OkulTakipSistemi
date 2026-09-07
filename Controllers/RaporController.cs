using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class RaporController : Controller
    {
        private readonly AppDbContext _context;

        public RaporController(AppDbContext context)
        {
            _context = context;
        }

        // RAPOR ANA SAYFA
        public IActionResult Index()
        {
            return View();
        }

        // ÖĞRENCİ RAPORU
        public IActionResult OgrenciRaporu()
        {
            var ogrenciler = _context.Ogrenciler
                .OrderBy(x => x.AdSoyad)
                .ToList();

            ViewBag.Ogrenciler = ogrenciler;

            return View();
        }

        // OKUL RAPORU
        public IActionResult OkulRaporu()
        {
            var okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            ViewBag.Okullar = okullar;

            return View();
        }

        // SINIF RAPORU
        public IActionResult SinifRaporu()
        {
            var siniflar = _context.Siniflar
                .Include(x => x.Okul)
                .OrderBy(x => x.Ad)
                .ToList();

            ViewBag.Siniflar = siniflar;

            return View();
        }

        // ÖDEME RAPORU
        public IActionResult OdemeRaporu()
        {
            var odemeler = _context.Odemeler
                .Include(x => x.Ogrenci)
                .Include(x => x.Kullanici)
                .Include(x => x.OdemeDagilimlari)
                    .ThenInclude(x => x.AylikBorc)
                .OrderByDescending(x => x.OdemeTarihi)
                .ToList();

            ViewBag.ToplamOdeme = odemeler.Sum(x => x.Tutar);
            ViewBag.OdemeSayisi = odemeler.Count;

            return View(odemeler);
        }

        // BORÇ RAPORU
        public IActionResult BorcRaporu()
        {
            var borclar = _context.AylikBorclar
                .Include(x => x.OgrenciKaydi)
                    .ThenInclude(x => x.Ogrenci)
                .Include(x => x.OgrenciKaydi)
                    .ThenInclude(x => x.Okul)
                .Include(x => x.OgrenciKaydi)
                    .ThenInclude(x => x.Sinif)
                .OrderBy(x => x.Yil)
                .ThenBy(x => x.Ay)
                .ToList();

            ViewBag.ToplamBorc = borclar.Sum(x => x.Tutar);
            ViewBag.ToplamOdenen = borclar.Sum(x => x.OdenenTutar);
            ViewBag.ToplamKalan = borclar.Sum(x => x.KalanTutar);

            return View(borclar);
        }

        // YOKLAMA RAPORU
        public IActionResult YoklamaRaporu()
        {
            var yoklamalar = _context.Yoklamalar
                .Include(x => x.Sinif)
                .Include(x => x.Ogretmen)
                .OrderByDescending(x => x.Tarih)
                .ToList();

            return View(yoklamalar);
        }

        // YOKLAMA DETAY RAPORU
        public IActionResult YoklamaDetay(int id)
        {
            var yoklama = _context.Yoklamalar
                .Include(x => x.Sinif)
                .Include(x => x.Ogretmen)
                .Include(x => x.Detaylar)
                    .ThenInclude(x => x.Ogrenci)
                .FirstOrDefault(x => x.Id == id);

            if (yoklama == null)
            {
                return NotFound();
            }

            return View(yoklama);
        }
    }
}
