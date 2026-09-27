using Microsoft.AspNetCore.Mvc;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class OkulController : Controller
    {
        private readonly AppDbContext _context;

        public OkulController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // OKULLARI LİSTELE
        // =========================

        public IActionResult Index()
        {
            var okullar = _context.Okullar
                .OrderBy(x => x.Id)
                .ToList();

            return View(okullar);
        }

        // =========================
        // OKUL EKLEME - SAYFA
        // =========================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =========================
        // OKUL EKLEME - İŞLEM
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Okul okul)
        {
            if (!ModelState.IsValid)
            {
                return View(okul);
            }

            _context.Okullar.Add(okul);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // OKUL GÜNCELLEME - SAYFA
        // =========================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var okul = _context.Okullar
                .FirstOrDefault(x => x.Id == id);

            if (okul == null)
            {
                return NotFound();
            }

            return View(okul);
        }

        // =========================
        // OKUL GÜNCELLEME - İŞLEM
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Okul model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var okul = _context.Okullar
                .FirstOrDefault(x => x.Id == model.Id);

            if (okul == null)
            {
                return NotFound();
            }

            okul.Ad = model.Ad;
            okul.Adres = model.Adres;
            okul.Telefon = model.Telefon;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // OKUL SİLME
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            Console.WriteLine("================================");
            Console.WriteLine("OKUL DELETE ÇAĞRILDI");
            Console.WriteLine("GELEN ID: " + id);

            var okul = _context.Okullar
                .FirstOrDefault(x => x.Id == id);

            if (okul == null)
            {
                Console.WriteLine("OKUL BULUNAMADI!");
                return NotFound();
            }

            Console.WriteLine("BULUNAN OKUL: " + okul.Ad);

            try
            {
                _context.Okullar.Remove(okul);
                _context.SaveChanges();

                Console.WriteLine("OKUL BAŞARIYLA SİLİNDİ!");

                TempData["DeleteSuccess"] =
                    okul.Ad + " okulu başarıyla silindi.";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                Console.WriteLine("DELETE HATASI: " + ex.InnerException?.Message);
                Console.WriteLine("ANA HATA: " + ex.Message);

                TempData["DeleteError"] =
                    okul.Ad + " okulu silinemedi. Bu okula bağlı kayıtlar bulunmaktadır.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
