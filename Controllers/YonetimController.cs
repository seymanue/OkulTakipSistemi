using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;

namespace OkulTakipSistemi.Controllers
{
    public class YonetimController : Controller
    {
        private readonly AppDbContext _context;

        public YonetimController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            ViewBag.Siniflar = _context.Siniflar
                .Include(x => x.Okul)
                .OrderBy(x => x.OkulId)
                .ThenBy(x => x.Ad)
                .ToList();

            ViewBag.Ogrenciler = _context.Ogrenciler
                .Include(x => x.Sinif)
                .ThenInclude(x => x.Okul)
                .OrderBy(x => x.AdSoyad)
                .ToList();

            ViewBag.OkulUcretleri = _context.OkulUcretleri
                .Include(x => x.Okul)
                .OrderBy(x => x.Okul!.Ad)
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult OkulUcretiKaydet(int okulId, decimal normalUcret)
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (okulId <= 0 || normalUcret <= 0)
            {
                TempData["Hata"] = "Okul ve geçerli bir normal ücret giriniz.";
                return RedirectToAction("Index");
            }

            var okul = _context.Okullar.FirstOrDefault(x => x.Id == okulId);

            if (okul == null)
            {
                TempData["Hata"] = "Okul bulunamadı.";
                return RedirectToAction("Index");
            }

            var mevcutUcret = _context.OkulUcretleri
                .FirstOrDefault(x => x.OkulId == okulId);

            if (mevcutUcret == null)
            {
                mevcutUcret = new Models.OkulUcreti
                {
                    OkulId = okulId
                };

                _context.OkulUcretleri.Add(mevcutUcret);
            }

            mevcutUcret.ToplamUcret = normalUcret;
            mevcutUcret.StandartVeliUcreti = normalUcret;
            mevcutUcret.OlusturmaTarihi = DateTime.Now;
            mevcutUcret.GecerlilikTarihi = DateTime.Today;

            _context.SaveChanges();

            TempData["Basari"] =
                $"{okul.Ad} için normal ücret {normalUcret:N2} TL olarak kaydedildi.";

            return RedirectToAction("Index");
        }
    }
}
