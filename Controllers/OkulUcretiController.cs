using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class OkulUcretiController : Controller
    {
        private readonly AppDbContext _context;

        public OkulUcretiController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // OKUL ÜCRETLERİ LİSTESİ
        // ============================================================

        [HttpGet]
        public IActionResult Index()
        {
            var ucretler = _context.OkulUcretleri
                .Include(x => x.Okul)
                .OrderByDescending(x => x.GecerlilikTarihi)
                .ThenByDescending(x => x.Id)
                .ToList();

            return View(ucretler);
        }

        // ============================================================
        // YENİ OKUL ÜCRETİ
        // ============================================================

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            return View();
        }

        // ============================================================
        // YENİ OKUL ÜCRETİ KAYDET
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OkulUcreti okulUcreti)
        {
            if (okulUcreti.OdemeSekli != "Pesin" &&
                okulUcreti.OdemeSekli != "Taksitli")
            {
                ModelState.AddModelError(
                    nameof(okulUcreti.OdemeSekli),
                    "Geçerli bir ödeme şekli seçiniz.");
            }

            if (okulUcreti.ToplamUcret <= 0)
            {
                ModelState.AddModelError(
                    nameof(okulUcreti.ToplamUcret),
                    "Toplam ücret 0'dan büyük olmalıdır.");
            }

            if (okulUcreti.OdemeSekli == "Pesin")
            {
                okulUcreti.TaksitAySayisi = 1;
            }
            else
            {
                if (okulUcreti.TaksitAySayisi < 1 ||
                    okulUcreti.TaksitAySayisi > 24)
                {
                    ModelState.AddModelError(
                        nameof(okulUcreti.TaksitAySayisi),
                        "Taksit sayısı 1 ile 24 arasında olmalıdır.");
                }
            }

            if (okulUcreti.BaslangicAyi < 1 ||
                okulUcreti.BaslangicAyi > 12)
            {
                ModelState.AddModelError(
                    nameof(okulUcreti.BaslangicAyi),
                    "Başlangıç ayı 1 ile 12 arasında olmalıdır.");
            }

            if (okulUcreti.GecerlilikTarihi == default)
            {
                okulUcreti.GecerlilikTarihi =
                    new DateTime(
                        DateTime.Now.Year,
                        okulUcreti.BaslangicAyi,
                        1);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Okullar = _context.Okullar
                    .OrderBy(x => x.Ad)
                    .ToList();

                return View(okulUcreti);
            }

            okulUcreti.OlusturmaTarihi = DateTime.Now;

            _context.OkulUcretleri.Add(okulUcreti);
            _context.SaveChanges();

            TempData["Basarili"] =
                "Okul ücreti başarıyla oluşturuldu.";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // ÜCRET DÜZENLEME SAYFASI
        // ============================================================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var okulUcreti = _context.OkulUcretleri
                .Include(x => x.Okul)
                .FirstOrDefault(x => x.Id == id);

            if (okulUcreti == null)
            {
                return NotFound();
            }

            ViewBag.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            return View(okulUcreti);
        }

        // ============================================================
        // ÜCRET DÜZENLEME KAYDET
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, OkulUcreti okulUcreti)
        {
            if (id != okulUcreti.Id)
            {
                return NotFound();
            }

            if (okulUcreti.OdemeSekli != "Pesin" &&
                okulUcreti.OdemeSekli != "Taksitli")
            {
                ModelState.AddModelError(
                    nameof(okulUcreti.OdemeSekli),
                    "Geçerli bir ödeme şekli seçiniz.");
            }

            if (okulUcreti.ToplamUcret <= 0)
            {
                ModelState.AddModelError(
                    nameof(okulUcreti.ToplamUcret),
                    "Toplam ücret 0'dan büyük olmalıdır.");
            }

            if (okulUcreti.OdemeSekli == "Pesin")
            {
                okulUcreti.TaksitAySayisi = 1;
            }
            else
            {
                if (okulUcreti.TaksitAySayisi < 1 ||
                    okulUcreti.TaksitAySayisi > 24)
                {
                    ModelState.AddModelError(
                        nameof(okulUcreti.TaksitAySayisi),
                        "Taksit sayısı 1 ile 24 arasında olmalıdır.");
                }
            }

            if (okulUcreti.BaslangicAyi < 1 ||
                okulUcreti.BaslangicAyi > 12)
            {
                ModelState.AddModelError(
                    nameof(okulUcreti.BaslangicAyi),
                    "Başlangıç ayı 1 ile 12 arasında olmalıdır.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Okullar = _context.Okullar
                    .OrderBy(x => x.Ad)
                    .ToList();

                return View(okulUcreti);
            }

            var mevcut = _context.OkulUcretleri
                .FirstOrDefault(x => x.Id == id);

            if (mevcut == null)
            {
                return NotFound();
            }

            mevcut.OkulId = okulUcreti.OkulId;
            mevcut.ToplamUcret = okulUcreti.ToplamUcret;
            mevcut.OdemeSekli = okulUcreti.OdemeSekli;
            mevcut.TaksitAySayisi = okulUcreti.TaksitAySayisi;
            mevcut.BaslangicAyi = okulUcreti.BaslangicAyi;
            mevcut.GecerlilikTarihi = okulUcreti.GecerlilikTarihi;
            mevcut.Aciklama = okulUcreti.Aciklama;

            _context.SaveChanges();

            TempData["Basarili"] =
                "Okul ücreti başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // OKUL ÜCRETİ SİL
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var ucret = _context.OkulUcretleri
                .FirstOrDefault(x => x.Id == id);

            if (ucret == null)
            {
                return NotFound();
            }

            _context.OkulUcretleri.Remove(ucret);
            _context.SaveChanges();

            TempData["Basarili"] =
                "Okul ücreti başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }
    }
}