using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class OgrenciUcretDurumuController : Controller
    {
        private readonly AppDbContext _context;

        public OgrenciUcretDurumuController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // LİSTE
        // ============================================================

        [HttpGet]
        public IActionResult Index()
        {
            var kayitlar = _context.OgrenciKayitlari
                .Include(x => x.Ogrenci)
                .Include(x => x.Okul)
                .Include(x => x.Sinif)
                .Where(x => x.Aktif)
                .ToList();

            var ucretDurumlari = _context.OgrenciUcretDurumlari
                .Include(x => x.OgrenciKaydi)
                    .ThenInclude(x => x.Ogrenci)
                .Include(x => x.OgrenciKaydi)
                    .ThenInclude(x => x.Okul)
                .Include(x => x.OgrenciKaydi)
                    .ThenInclude(x => x.Sinif)
                .ToList();

            var sonuc = kayitlar.Select(kayit =>
            {
                var durum = ucretDurumlari
                    .FirstOrDefault(x => x.OgrenciKaydiId == kayit.Id);

                if (durum != null)
                {
                    return durum;
                }

                return new OgrenciUcretDurumu
                {
                    Id = 0,
                    OgrenciKaydiId = kayit.Id,
                    OgrenciKaydi = kayit,
                    Indirimli = false,
                    Ucretsiz = false
                };
            }).ToList();

            return View(sonuc);
        }

        // ============================================================
        // YENİ ÜCRET DURUMU
        // ============================================================

        [HttpGet]
        public IActionResult Create()
        {
            var ogrenciKayitlari = _context.OgrenciKayitlari
                .Include(x => x.Ogrenci)
                .Include(x => x.Okul)
                .Include(x => x.Sinif)
                .Where(x => x.Aktif)
                .ToList();

            ViewBag.OgrenciKayitlari = ogrenciKayitlari;

            return View(new OgrenciUcretDurumu());
        }

        // ============================================================
        // ÜCRET DURUMU KAYDET
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OgrenciUcretDurumu model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.OgrenciKayitlari = _context.OgrenciKayitlari
                    .Include(x => x.Ogrenci)
                    .Include(x => x.Okul)
                    .Include(x => x.Sinif)
                    .Where(x => x.Aktif)
                    .ToList();

                return View(model);
            }

            var mevcut = _context.OgrenciUcretDurumlari
                .FirstOrDefault(x =>
                    x.OgrenciKaydiId == model.OgrenciKaydiId);

            if (mevcut != null)
            {
                ModelState.AddModelError(
                    "",
                    "Bu öğrenci kaydı için zaten bir ücret durumu bulunmaktadır.");

                ViewBag.OgrenciKayitlari = _context.OgrenciKayitlari
                    .Include(x => x.Ogrenci)
                    .Include(x => x.Okul)
                    .Include(x => x.Sinif)
                    .Where(x => x.Aktif)
                    .ToList();

                return View(model);
            }

            _context.OgrenciUcretDurumlari.Add(model);
            _context.SaveChanges();

            TempData["Basarili"] =
                "Öğrencinin ücret durumu başarıyla oluşturuldu.";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // SİL
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var durum = _context.OgrenciUcretDurumlari
                .FirstOrDefault(x => x.Id == id);

            if (durum == null)
            {
                TempData["Hata"] =
                    "Silinecek ücret durumu bulunamadı.";

                return RedirectToAction(nameof(Index));
            }

            _context.OgrenciUcretDurumlari.Remove(durum);
            _context.SaveChanges();

            TempData["Basarili"] =
                "Ücret durumu silindi.";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // ÖDEME PLANI OLUŞTURMA SAYFASI
        // ============================================================

        [HttpGet]
        public IActionResult OdemePlani(int id)
        {
            var kayit = _context.OgrenciKayitlari
                .Include(x => x.Ogrenci)
                .Include(x => x.Okul)
                .Include(x => x.Sinif)
                .FirstOrDefault(x => x.Id == id);

            if (kayit == null)
            {
                TempData["Hata"] =
                    "Öğrenci kaydı bulunamadı.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // ÖĞRENCİNİN ÜCRET DURUMU
            // --------------------------------------------------------

            var ucretDurumu = _context.OgrenciUcretDurumlari
                .FirstOrDefault(x =>
                    x.OgrenciKaydiId == id);

            if (ucretDurumu == null)
            {
                TempData["Hata"] =
                    "Önce bu öğrenci için ücret durumu oluşturmalısınız.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // OKUL ÜCRETİ
            // --------------------------------------------------------

            var okulUcreti = _context.OkulUcretleri
                .Where(x => x.OkulId == kayit.OkulId)
                .OrderByDescending(x => x.GecerlilikTarihi)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            if (okulUcreti == null)
            {
                TempData["Hata"] =
                    $"'{kayit.Okul?.Ad}' okulu için ücret tanımlanmamış.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // OKUL ÜCRETİNDEKİ TAKSİT SAYISI
            // --------------------------------------------------------

            int taksitSayisi = okulUcreti.TaksitAySayisi;

            if (taksitSayisi < 1)
            {
                taksitSayisi = 1;
            }

            // --------------------------------------------------------
            // AYLIK ÜCRET
            // --------------------------------------------------------

            decimal aylikUcret;

            if (okulUcreti.OdemeSekli == "Pesin")
            {
                aylikUcret = okulUcreti.ToplamUcret;
                taksitSayisi = 1;
            }
            else
            {
                aylikUcret = Math.Round(
                    okulUcreti.ToplamUcret / taksitSayisi,
                    2);
            }

            // --------------------------------------------------------
            // ÖĞRENCİYE UYGULANACAK ÜCRET
            // --------------------------------------------------------

            decimal aylikOdenecekTutar = aylikUcret;

            if (ucretDurumu.Ucretsiz)
            {
                aylikOdenecekTutar = 0m;
            }
            else if (ucretDurumu.OzelFiyat.HasValue)
            {
                aylikOdenecekTutar =
                    ucretDurumu.OzelFiyat.Value;
            }
            else if (ucretDurumu.Indirimli)
            {
                aylikOdenecekTutar =
                    aylikUcret -
                    (
                        aylikUcret *
                        ucretDurumu.IndirimOrani /
                        100m
                    );

                aylikOdenecekTutar =
                    Math.Round(
                        aylikOdenecekTutar,
                        2);
            }

            // --------------------------------------------------------
            // BAŞLANGIÇ TARİHİ
            // --------------------------------------------------------

            int baslangicYili;

            if (okulUcreti.GecerlilikTarihi != default)
            {
                baslangicYili =
                    okulUcreti.GecerlilikTarihi.Year;
            }
            else
            {
                baslangicYili =
                    DateTime.Now.Year;
            }

            int baslangicAyi =
                okulUcreti.BaslangicAyi;

            if (baslangicAyi < 1 ||
                baslangicAyi > 12)
            {
                baslangicAyi =
                    DateTime.Now.Month;
            }

            // --------------------------------------------------------
            // VIEW MODEL
            // --------------------------------------------------------

            var model = new OdemePlaniViewModel
            {
                OgrenciKaydiId = kayit.Id,

                OgrenciAdi =
                    kayit.Ogrenci?.AdSoyad ?? "",

                OkulAdi =
                    kayit.Okul?.Ad ?? "",

                SinifAdi =
                    kayit.Sinif?.Ad ?? "",

                AylikUcret =
                    aylikUcret,

                AylikOdenecekTutar =
                    aylikOdenecekTutar,

                BaslangicYili =
                    baslangicYili,

                BaslangicAyi =
                    baslangicAyi,

                TaksitSayisi =
                    taksitSayisi
            };

            return View(model);
        }

        // ============================================================
        // ÖDEME PLANI KAYDET
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OdemePlani(
            OdemePlaniViewModel model)
        {
            // --------------------------------------------------------
            // ÖĞRENCİ KAYDI
            // --------------------------------------------------------

            var kayit = _context.OgrenciKayitlari
                .Include(x => x.Ogrenci)
                .Include(x => x.Okul)
                .Include(x => x.Sinif)
                .FirstOrDefault(
                    x => x.Id == model.OgrenciKaydiId);

            if (kayit == null)
            {
                TempData["Hata"] =
                    "Öğrenci kaydı bulunamadı.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // TARİH KONTROLLERİ
            // --------------------------------------------------------

            if (model.BaslangicYili < 2020 ||
                model.BaslangicYili > 2100)
            {
                TempData["Hata"] =
                    "Başlangıç yılı geçersiz.";

                return RedirectToAction(
                    nameof(OdemePlani),
                    new { id = model.OgrenciKaydiId });
            }

            if (model.BaslangicAyi < 1 ||
                model.BaslangicAyi > 12)
            {
                TempData["Hata"] =
                    "Başlangıç ayı geçersiz.";

                return RedirectToAction(
                    nameof(OdemePlani),
                    new { id = model.OgrenciKaydiId });
            }

            if (model.TaksitSayisi < 1 ||
                model.TaksitSayisi > 24)
            {
                TempData["Hata"] =
                    "Taksit sayısı 1 ile 24 arasında olmalıdır.";

                return RedirectToAction(
                    nameof(OdemePlani),
                    new { id = model.OgrenciKaydiId });
            }

            // --------------------------------------------------------
            // ÜCRET DURUMU
            // --------------------------------------------------------

            var ucretDurumu =
                _context.OgrenciUcretDurumlari
                    .FirstOrDefault(
                        x =>
                            x.OgrenciKaydiId ==
                            model.OgrenciKaydiId);

            if (ucretDurumu == null)
            {
                TempData["Hata"] =
                    "Bu öğrenci için ücret durumu bulunamadı.";

                return RedirectToAction(
                    nameof(OdemePlani),
                    new { id = model.OgrenciKaydiId });
            }

            // --------------------------------------------------------
            // OKUL ÜCRETİ
            // --------------------------------------------------------

            var okulUcreti = _context.OkulUcretleri
                .Where(x => x.OkulId == kayit.OkulId)
                .OrderByDescending(x => x.GecerlilikTarihi)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            if (okulUcreti == null)
            {
                TempData["Hata"] =
                    $"'{kayit.Okul?.Ad}' okulu için ücret tanımlanmamış.";

                return RedirectToAction(
                    nameof(OdemePlani),
                    new { id = model.OgrenciKaydiId });
            }

            // --------------------------------------------------------
            // DAHA ÖNCE PLAN OLUŞTURULMUŞ MU?
            // --------------------------------------------------------

            bool planVarMi =
                _context.AylikBorclar.Any(
                    x =>
                        x.OgrenciKaydiId ==
                        model.OgrenciKaydiId);

            if (planVarMi)
            {
                TempData["Hata"] =
                    "Bu öğrenci için daha önce ödeme planı oluşturulmuş.";

                return RedirectToAction(
                    nameof(OdemePlaniListe),
                    new { id = model.OgrenciKaydiId });
            }

            // --------------------------------------------------------
            // AYLIK ÜCRET
            // --------------------------------------------------------

            decimal aylikUcret;

            if (okulUcreti.OdemeSekli == "Pesin")
            {
                aylikUcret =
                    okulUcreti.ToplamUcret;
            }
            else
            {
                /*
                 * Burada artık okulun eski taksit sayısını
                 * kullanmıyoruz.
                 *
                 * Kullanıcının Ödeme Planı ekranında seçtiği
                 * TaksitSayisi kullanılacak.
                 */

                aylikUcret =
                    Math.Round(
                        okulUcreti.ToplamUcret /
                        model.TaksitSayisi,
                        2);
            }

            // --------------------------------------------------------
            // ÖĞRENCİ İNDİRİMİ / ÖZEL FİYAT
            // --------------------------------------------------------

            decimal aylikOdenecekTutar =
                aylikUcret;

            if (ucretDurumu.Ucretsiz)
            {
                aylikOdenecekTutar = 0m;
            }
            else if (ucretDurumu.OzelFiyat.HasValue)
            {
                aylikOdenecekTutar =
                    ucretDurumu.OzelFiyat.Value;
            }
            else if (ucretDurumu.Indirimli)
            {
                aylikOdenecekTutar =
                    aylikUcret -
                    (
                        aylikUcret *
                        ucretDurumu.IndirimOrani /
                        100m
                    );

                aylikOdenecekTutar =
                    Math.Round(
                        aylikOdenecekTutar,
                        2);
            }

            // --------------------------------------------------------
            // AYLIK BORÇLARI OLUŞTUR
            // --------------------------------------------------------

            var yeniBorclar =
                new List<AylikBorc>();

            DateTime tarih =
                new DateTime(
                    model.BaslangicYili,
                    model.BaslangicAyi,
                    1);

            for (int i = 0;
                 i < model.TaksitSayisi;
                 i++)
            {
                DateTime borcTarihi =
                    tarih.AddMonths(i);

                decimal tutar =
                    aylikOdenecekTutar;

                /*
                 * Son taksitte kuruş farkı oluşursa
                 * toplam ücretin tam karşılanması için
                 * farkı son taksite ekliyoruz.
                 */

                if (i == model.TaksitSayisi - 1)
                {
                    decimal toplam =
                        aylikOdenecekTutar *
                        model.TaksitSayisi;

                    decimal fark =
                        okulUcreti.ToplamUcret -
                        toplam;

                    if (!ucretDurumu.OzelFiyat.HasValue &&
                        !ucretDurumu.Ucretsiz &&
                        !ucretDurumu.Indirimli)
                    {
                        tutar += fark;
                    }
                }

                var borc = new AylikBorc
                {
                    OgrenciKaydiId =
                        model.OgrenciKaydiId,

                    Yil =
                        borcTarihi.Year,

                    Ay =
                        borcTarihi.Month,

                    Tutar =
                        tutar,

                    OdenenTutar =
                        0m,

                    Aciklama =
                        "Otomatik oluşturulan ödeme planı"
                };

                yeniBorclar.Add(borc);
            }

            // --------------------------------------------------------
            // VERİTABANINA KAYDET
            // --------------------------------------------------------

            try
            {
                _context.AylikBorclar.AddRange(
                    yeniBorclar);

                _context.SaveChanges();

                TempData["Basarili"] =
                    $"{yeniBorclar.Count} aylık ödeme planı oluşturuldu.";

                return RedirectToAction(
                    nameof(OdemePlaniListe),
                    new
                    {
                        id = model.OgrenciKaydiId
                    });
            }
            catch (Exception ex)
            {
                TempData["Hata"] =
                    "Ödeme planı kaydedilirken hata oluştu: " +
                    ex.Message;

                return RedirectToAction(
                    nameof(OdemePlani),
                    new
                    {
                        id = model.OgrenciKaydiId
                    });
            }
        }

        // ============================================================
        // ÖDEME PLANI LİSTESİ
        // ============================================================

        [HttpGet]
        public IActionResult OdemePlaniListe(int id)
        {
            var kayit = _context.OgrenciKayitlari
                .Include(x => x.Ogrenci)
                .Include(x => x.Okul)
                .Include(x => x.Sinif)
                .FirstOrDefault(x => x.Id == id);

            if (kayit == null)
            {
                TempData["Hata"] =
                    "Öğrenci kaydı bulunamadı.";

                return RedirectToAction(nameof(Index));
            }

            var borclar = _context.AylikBorclar
                .Where(x =>
                    x.OgrenciKaydiId == id)
                .OrderBy(x => x.Yil)
                .ThenBy(x => x.Ay)
                .ToList();

            ViewBag.Ogrenci =
                kayit;

            return View(borclar);
        }
    }
}