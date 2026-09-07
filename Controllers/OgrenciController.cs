using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;

namespace OkulTakipSistemi.Controllers
{
    public class OgrenciController : Controller
    {
        private readonly AppDbContext _context;

        public OgrenciController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // ÖĞRENCİLER LİSTESİ
        // =========================================================
        public IActionResult Index()
        {
            var ogrenciler = _context.Ogrenciler
                .Include(x => x.Sinif)
                .ThenInclude(x => x!.Okul)
                .ToList();

            ViewBag.Kayitlar = _context.OgrenciKayitlari
                .Include(x => x.Ogrenci)
                .Include(x => x.Sinif)
                .Include(x => x.Okul)
                .ToList();

            ViewBag.OgretmenSiniflar = _context.OgretmenSiniflar
                .Include(x => x.Ogretmen)
                .Include(x => x.Sinif)
                .ToList();

            return View(ogrenciler);
        }


        // =========================================================
        // YENİ ÖĞRENCİ KAYDI - GET
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            ViewBag.Siniflar = _context.Siniflar
                .Include(x => x.Okul)
                .Where(x => x.Aktif)
                .OrderBy(x => x.Ad)
                .ToList();

            return View();
        }


        // =========================================================
        // YENİ ÖĞRENCİ KAYDI - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Ogrenci ogrenci)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Okullar = _context.Okullar
                    .OrderBy(x => x.Ad)
                    .ToList();

                ViewBag.Siniflar = _context.Siniflar
                    .Include(x => x.Okul)
                    .Where(x => x.Aktif)
                    .OrderBy(x => x.Ad)
                    .ToList();

                return View(ogrenci);
            }

            // Öğrenci aktif olarak oluşturuluyor
            ogrenci.Aktif = true;

            // Kayıt tarihi boşsa bugünün tarihi
            if (ogrenci.KayitTarihi == default)
            {
                ogrenci.KayitTarihi = DateTime.Now;
            }

            // ---------------------------------------------------------
            // 1. ÖĞRENCİYİ Ogrenciler TABLOSUNA EKLE
            // ---------------------------------------------------------
            _context.Ogrenciler.Add(ogrenci);
            _context.SaveChanges();


            // ---------------------------------------------------------
            // 2. ÖĞRENCİNİN SINIFI VARSA OgrenciKaydi OLUŞTUR
            // ---------------------------------------------------------
            if (ogrenci.SinifId.HasValue)
            {
                var sinif = _context.Siniflar
                    .FirstOrDefault(x => x.Id == ogrenci.SinifId.Value);

                if (sinif != null)
                {
                    var mevcutKayit = _context.OgrenciKayitlari
                        .FirstOrDefault(x =>
                            x.OgrenciId == ogrenci.Id &&
                            x.Aktif);

                    // Daha önce kayıt yoksa oluştur
                    if (mevcutKayit == null)
                    {
                        var ogrenciKaydi = new OgrenciKaydi
                        {
                            OgrenciId = ogrenci.Id,
                            OkulId = sinif.OkulId,
                            SinifId = sinif.Id,
                            Aktif = true
                        };

                        _context.OgrenciKayitlari.Add(ogrenciKaydi);
                        _context.SaveChanges();
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // ÖĞRENCİ DÜZENLE - GET
        // =========================================================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var ogrenci = _context.Ogrenciler
                .Include(x => x.Sinif)
                .FirstOrDefault(x => x.Id == id);

            if (ogrenci == null)
            {
                return NotFound();
            }

            ViewBag.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            ViewBag.Siniflar = _context.Siniflar
                .Include(x => x.Okul)
                .Where(x => x.Aktif)
                .OrderBy(x => x.Ad)
                .ToList();

            return View(ogrenci);
        }


        // =========================================================
        // ÖĞRENCİ DÜZENLE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Ogrenci ogrenci)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Okullar = _context.Okullar
                    .OrderBy(x => x.Ad)
                    .ToList();

                ViewBag.Siniflar = _context.Siniflar
                    .Include(x => x.Okul)
                    .Where(x => x.Aktif)
                    .OrderBy(x => x.Ad)
                    .ToList();

                return View(ogrenci);
            }

            var mevcutOgrenci = _context.Ogrenciler
                .FirstOrDefault(x => x.Id == ogrenci.Id);

            if (mevcutOgrenci == null)
            {
                return NotFound();
            }

            // Öğrenci bilgilerini güncelle
            mevcutOgrenci.AdSoyad = ogrenci.AdSoyad;
            mevcutOgrenci.OgrenciNo = ogrenci.OgrenciNo;
            mevcutOgrenci.DogumTarihi = ogrenci.DogumTarihi;
            mevcutOgrenci.Cinsiyet = ogrenci.Cinsiyet;
            mevcutOgrenci.Telefon = ogrenci.Telefon;
            mevcutOgrenci.Adres = ogrenci.Adres;
            mevcutOgrenci.SinifId = ogrenci.SinifId;
            mevcutOgrenci.Aktif = ogrenci.Aktif;

            _context.SaveChanges();


            // ---------------------------------------------------------
            // ÖĞRENCİNİN OgrenciKaydi KAYDINI DA GÜNCELLE
            // ---------------------------------------------------------
            if (ogrenci.SinifId.HasValue)
            {
                var sinif = _context.Siniflar
                    .FirstOrDefault(x => x.Id == ogrenci.SinifId.Value);

                if (sinif != null)
                {
                    var kayit = _context.OgrenciKayitlari
                        .FirstOrDefault(x =>
                            x.OgrenciId == ogrenci.Id &&
                            x.Aktif);

                    if (kayit == null)
                    {
                        // Kayıt yoksa oluştur
                        kayit = new OgrenciKaydi
                        {
                            OgrenciId = ogrenci.Id,
                            OkulId = sinif.OkulId,
                            SinifId = sinif.Id,
                            Aktif = true
                        };

                        _context.OgrenciKayitlari.Add(kayit);
                    }
                    else
                    {
                        // Kayıt varsa sınıf ve okulu güncelle
                        kayit.OkulId = sinif.OkulId;
                        kayit.SinifId = sinif.Id;
                        kayit.Aktif = true;
                    }

                    _context.SaveChanges();
                }
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // AKTİF / PASİF DEĞİŞTİR
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleAktif(int id)
        {
            var ogrenci = _context.Ogrenciler
                .FirstOrDefault(x => x.Id == id);

            if (ogrenci == null)
            {
                return NotFound();
            }

            ogrenci.Aktif = !ogrenci.Aktif;

            // Öğrenci pasif olursa kayıt da pasif olsun
            var kayitlar = _context.OgrenciKayitlari
                .Where(x => x.OgrenciId == id)
                .ToList();

            foreach (var kayit in kayitlar)
            {
                kayit.Aktif = ogrenci.Aktif;
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // ÖĞRENCİ SİL
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var ogrenci = _context.Ogrenciler
                .FirstOrDefault(x => x.Id == id);

            if (ogrenci == null)
            {
                return NotFound();
            }


            // ---------------------------------------------------------
            // ÖĞRENCİ KAYITLARINI SİL
            // ---------------------------------------------------------
            var kayitlar = _context.OgrenciKayitlari
                .Where(x => x.OgrenciId == id)
                .ToList();

            if (kayitlar.Any())
            {
                _context.OgrenciKayitlari.RemoveRange(kayitlar);
            }


            // ---------------------------------------------------------
            // VELİ İLİŞKİLERİNİ SİL
            // ---------------------------------------------------------
            var veliIliskileri = _context.OgrenciVeliler
                .Where(x => x.OgrenciId == id)
                .ToList();

            if (veliIliskileri.Any())
            {
                _context.OgrenciVeliler.RemoveRange(veliIliskileri);
            }


            // ---------------------------------------------------------
            // ÖĞRENCİYİ SİL
            // ---------------------------------------------------------
            _context.Ogrenciler.Remove(ogrenci);

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}