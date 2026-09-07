using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;

namespace OkulTakipSistemi.Controllers
{
    public class AylikBorcController : Controller
    {
        private readonly AppDbContext _context;

        public AylikBorcController(AppDbContext context)
        {
            _context = context;
        }

        // Tüm ödeme planı
        public IActionResult Index()
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

            return View(borclar);
        }

        // Belirli öğrencinin ödeme planı
        public IActionResult Ogrenci(int id)
        {
            var borclar = _context.AylikBorclar
                .Include(x => x.OgrenciKaydi)
                    .ThenInclude(x => x.Ogrenci)
                .Include(x => x.OgrenciKaydi)
                    .ThenInclude(x => x.Okul)
                .Include(x => x.OgrenciKaydi)
                    .ThenInclude(x => x.Sinif)
                .Where(x => x.OgrenciKaydiId == id)
                .OrderBy(x => x.Yil)
                .ThenBy(x => x.Ay)
                .ToList();

            return View("Ogrenci", borclar);
        }
    }
}