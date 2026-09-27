using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class UcretYonetimiController : Controller
    {
        private readonly AppDbContext _context;

        public UcretYonetimiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(int? okulId)
        {
            if (!YoneticiMi())
                return RedirectToAction("Login", "Account");

            var model = new UcretYonetimiViewModel
            {
                SeciliOkulId = okulId,

                Okullar = _context.Okullar
                    .OrderBy(x => x.Ad)
                    .ToList()
            };

            if (okulId.HasValue && okulId.Value > 0)
            {
                model.OkulUcreti = _context.OkulUcretleri
                    .Where(x => x.OkulId == okulId.Value)
                    .OrderByDescending(x => x.GecerlilikTarihi)
                    .ThenByDescending(x => x.Id)
                    .FirstOrDefault();
            }

            return View(model);
        }

        // OKUL ÜCRETİNİ KAYDET
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OkulUcretiKaydet(
            int okulId,
            decimal toplamUcret,
            decimal standartVeliUcreti,
            string odemeSekli,
            int taksitAySayisi,
            int baslangicAyi,
            string? aciklama)
        {
            if (!YoneticiMi())
                return RedirectToAction("Login", "Account");

            if (okulId <= 0)
            {
                TempData["Hata"] = "Lütfen bir okul seçiniz.";
                return RedirectToAction(nameof(Index));
            }

            if (toplamUcret <= 0 || standartVeliUcreti <= 0)
            {
                TempData["Hata"] =
                    "Kurum ücreti ve standart veli ücreti 0'dan büyük olmalıdır.";

                return RedirectToAction(nameof(Index),
                    new { okulId });
            }

            if (odemeSekli != "Pesin" && odemeSekli != "Taksitli")
            {
                TempData["Hata"] = "Geçerli bir ödeme şekli seçiniz.";

                return RedirectToAction(nameof(Index),
                    new { okulId });
            }

            if (odemeSekli == "Taksitli" &&
                (taksitAySayisi < 1 || taksitAySayisi > 8))
            {
                TempData["Hata"] =
                    "Taksit sayısı 1 ile 8 arasında olmalıdır.";

                return RedirectToAction(nameof(Index),
                    new { okulId });
            }

            if (odemeSekli == "Pesin")
            {
                taksitAySayisi = 1;
            }

            if (baslangicAyi < 1 || baslangicAyi > 12)
            {
                TempData["Hata"] = "Geçerli bir başlangıç ayı seçiniz.";

                return RedirectToAction(nameof(Index),
                    new { okulId });
            }

            var mevcut = _context.OkulUcretleri
                .Where(x => x.OkulId == okulId)
                .OrderByDescending(x => x.GecerlilikTarihi)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            if (mevcut == null)
            {
                mevcut = new OkulUcreti
                {
                    OkulId = okulId,
                    OlusturmaTarihi = DateTime.Now
                };

                _context.OkulUcretleri.Add(mevcut);
            }

            mevcut.ToplamUcret = toplamUcret;
            mevcut.KurumIcinBelirlenenUcret = toplamUcret;
            mevcut.StandartVeliUcreti = standartVeliUcreti;
            mevcut.OdemeSekli = odemeSekli;
            mevcut.TaksitAySayisi = taksitAySayisi;
            mevcut.BaslangicAyi = baslangicAyi;
            mevcut.GecerlilikTarihi =
                new DateTime(DateTime.Today.Year, baslangicAyi, 1);
            mevcut.Aciklama = aciklama;

            _context.SaveChanges();

            TempData["Basarili"] =
                "Okulun ücret bilgileri başarıyla kaydedildi.";

            return RedirectToAction(nameof(Index),
                new { okulId });
        }

        private bool YoneticiMi()
        {
            return HttpContext.Session.GetString("Rol") == "Admin";
        }
    }
}
