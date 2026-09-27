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
        public IActionResult Index(int? okulId, int? sinifId)
        {
            ViewBag.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            ViewBag.Siniflar = okulId.HasValue
                ? _context.Siniflar
                    .Where(x => x.OkulId == okulId.Value && x.Aktif)
                    .OrderBy(x => x.Ad)
                    .ToList()
                : new List<Sinif>();

            ViewBag.SeciliOkulId = okulId;
            ViewBag.SeciliSinifId = sinifId;

            var ogrencilerQuery = _context.Ogrenciler
                .Include(x => x.Sinif)
                .AsQueryable();

            if (okulId.HasValue)
            {
                ogrencilerQuery = ogrencilerQuery
                    .Where(x => x.Sinif != null &&
                                x.Sinif.OkulId == okulId.Value);
            }

            if (sinifId.HasValue)
            {
                ogrencilerQuery = ogrencilerQuery
                    .Where(x => x.SinifId == sinifId.Value);
            }

            var ogrenciler = ogrencilerQuery
                .OrderBy(x => x.AdSoyad)
                .ToList();

            var ogrenciIdleri = ogrenciler
                .Select(x => x.Id)
                .ToList();

            ViewBag.Veliler = _context.OgrenciVeliler
                .Include(x => x.Veli)
                .Where(x => ogrenciIdleri.Contains(x.OgrenciId))
                .ToList();

            return View(ogrenciler);
        }

        // =========================================================
        // YENİ ÖĞRENCİ KAYDI - GET
        // =========================================================
        [HttpGet]
        public IActionResult Create(int? sinifId)
        {
            ViewBag.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            ViewBag.Siniflar = _context.Siniflar
                .Include(x => x.Okul)
                .Where(x => x.Aktif)
                .OrderBy(x => x.Ad)
                .ToList();

            if (sinifId.HasValue)
            {
                var sinif = _context.Siniflar
                    .FirstOrDefault(x => x.Id == sinifId.Value);

                if (sinif != null)
                {
                    var ogrenci = new Ogrenci
                    {
                        SinifId = sinif.Id
                    };

                    ViewBag.SeciliSinif = sinif;
                    return View(ogrenci);
                }
            }

            return View(new Ogrenci());
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
        // BİRLEŞİK ÖĞRENCİ KAYDI - GET
        // =========================================================
        [HttpGet]
        public IActionResult Kayit()
        {
            if (HttpContext.Session.GetString("Rol") != "Admin")
                return RedirectToAction("Login", "Account");

            var model = new OgrenciKayitFormViewModel
            {
                Okullar = _context.Okullar
                    .OrderBy(x => x.Ad)
                    .ToList(),

                Siniflar = _context.Siniflar
                    .Include(x => x.Okul)
                    .Where(x => x.Aktif)
                    .OrderBy(x => x.OkulId)
                    .ThenBy(x => x.Ad)
                    .ToList(),

                OkulUcretleri = _context.OkulUcretleri
                    .ToList()
                    .GroupBy(x => x.OkulId)
                    .Select(g => g
                        .OrderByDescending(x => x.GecerlilikTarihi)
                        .ThenByDescending(x => x.Id)
                        .First())
                    .ToList()
            };

            return View("Kayit", model);
        }


        // =========================================================
        // BİRLEŞİK ÖĞRENCİ KAYDI - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Kayit(OgrenciKayitFormViewModel model)
        {
            if (HttpContext.Session.GetString("Rol") != "Admin")
                return RedirectToAction("Login", "Account");

            model.Okullar = _context.Okullar
                .OrderBy(x => x.Ad)
                .ToList();

            model.Siniflar = _context.Siniflar
                .Include(x => x.Okul)
                .Where(x => x.Aktif)
                .OrderBy(x => x.OkulId)
                .ThenBy(x => x.Ad)
                .ToList();

            model.OkulUcretleri = _context.OkulUcretleri
                .ToList()
                .GroupBy(x => x.OkulId)
                .Select(g => g
                    .OrderByDescending(x => x.GecerlilikTarihi)
                    .ThenByDescending(x => x.Id)
                    .First())
                .ToList();

            if (string.IsNullOrWhiteSpace(model.AdSoyad))
                ModelState.AddModelError("AdSoyad", "Öğrenci adı soyadı zorunludur.");

            if (string.IsNullOrWhiteSpace(model.OgrenciNo))
                ModelState.AddModelError("OgrenciNo", "Öğrenci numarası zorunludur.");

            if (model.OkulId <= 0)
                ModelState.AddModelError("OkulId", "Okul seçiniz.");

            if (model.SinifId <= 0)
                ModelState.AddModelError("SinifId", "Sınıf seçiniz.");

            if (string.IsNullOrWhiteSpace(model.VeliAdSoyad))
                ModelState.AddModelError("VeliAdSoyad", "Veli adı soyadı zorunludur.");

            if (string.IsNullOrWhiteSpace(model.VeliTelefon))
                ModelState.AddModelError("VeliTelefon", "Veli telefonu zorunludur.");

            if (model.Ucretsiz)
            {
                model.OdemeSekli = "Taksitli";
                model.TaksitSayisi = 0;
            }
            else
            {
                if (model.OdemeSekli == "Pesin")
                {
                    model.TaksitSayisi = 1;
                }
                else
                {
                    if (model.TaksitSayisi < 1)
                        model.TaksitSayisi = 8;

                    if (model.TaksitSayisi > 8)
                        model.TaksitSayisi = 8;
                }

                if (model.Indirimli &&
                    (model.IndirimOrani < 0 || model.IndirimOrani > 100))
                {
                    ModelState.AddModelError(
                        "IndirimOrani",
                        "İndirim oranı 0 ile 100 arasında olmalıdır.");
                }

                if (model.OzelFiyat.HasValue && model.OzelFiyat.Value <= 0)
                {
                    ModelState.AddModelError(
                        "OzelFiyat",
                        "Özel fiyat 0'dan büyük olmalıdır.");
                }
            }

            var sinif = _context.Siniflar
                .FirstOrDefault(x =>
                    x.Id == model.SinifId &&
                    x.Aktif);

            if (sinif == null)
            {
                ModelState.AddModelError("SinifId", "Geçerli bir sınıf seçiniz.");
            }
            else if (sinif.OkulId != model.OkulId)
            {
                ModelState.AddModelError(
                    "SinifId",
                    "Seçilen sınıf, seçilen okula ait değil.");
            }

            var ogrenciNoVarMi = _context.Ogrenciler
                .Any(x => x.OgrenciNo == model.OgrenciNo);

            if (ogrenciNoVarMi)
            {
                ModelState.AddModelError(
                    "OgrenciNo",
                    "Bu öğrenci numarası zaten kullanılıyor.");
            }

            var okulUcreti = _context.OkulUcretleri
                .Where(x => x.OkulId == model.OkulId)
                .OrderByDescending(x => x.GecerlilikTarihi)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            if (!model.Ucretsiz &&
                !model.OzelFiyat.HasValue &&
                okulUcreti == null)
            {
                ModelState.AddModelError(
                    "OzelFiyat",
                    "Bu okul için ücret tanımlanmamış. Özel fiyat giriniz veya önce okul ücretini tanımlayınız.");
            }

            if (!ModelState.IsValid)
                return View("Kayit", model);

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // -------------------------------------------------
                // 1. ÖĞRENCİ
                // -------------------------------------------------
                var ogrenci = new Ogrenci
                {
                    OgrenciNo = model.OgrenciNo.Trim(),
                    AdSoyad = model.AdSoyad.Trim(),
                    DogumTarihi = model.DogumTarihi,
                    Cinsiyet = model.Cinsiyet?.Trim() ?? string.Empty,
                    Telefon = model.Telefon?.Trim() ?? string.Empty,
                    Adres = model.Adres?.Trim() ?? string.Empty,
                    KayitTarihi = DateTime.Now,
                    Aktif = true,
                    SinifId = model.SinifId
                };

                _context.Ogrenciler.Add(ogrenci);
                _context.SaveChanges();

                // -------------------------------------------------
                // 2. ÖĞRENCİ KAYDI
                // -------------------------------------------------
                var ogrenciKaydi = new OgrenciKaydi
                {
                    OgrenciId = ogrenci.Id,
                    OkulId = model.OkulId,
                    SinifId = model.SinifId,
                    Aktif = true
                };

                _context.OgrenciKayitlari.Add(ogrenciKaydi);
                _context.SaveChanges();

                // -------------------------------------------------
                // 3. VELİ
                // Aynı telefon varsa mevcut veli kullanılır.
                // -------------------------------------------------
                var veli = _context.Veliler
                    .FirstOrDefault(x =>
                        x.Telefon == model.VeliTelefon.Trim());

                if (veli == null)
                {
                    veli = new Veli
                    {
                        AdSoyad = model.VeliAdSoyad.Trim(),
                        Telefon = model.VeliTelefon.Trim(),
                        Yakinlik = model.VeliYakinlik?.Trim() ?? string.Empty
                    };

                    _context.Veliler.Add(veli);
                    _context.SaveChanges();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(veli.Yakinlik))
                        veli.Yakinlik = model.VeliYakinlik?.Trim() ?? string.Empty;

                    _context.SaveChanges();
                }

                var veliBaglantisiVarMi = _context.OgrenciVeliler
                    .Any(x =>
                        x.OgrenciId == ogrenci.Id &&
                        x.VeliId == veli.Id);

                if (!veliBaglantisiVarMi)
                {
                    _context.OgrenciVeliler.Add(new OgrenciVeli
                    {
                        OgrenciId = ogrenci.Id,
                        VeliId = veli.Id
                    });

                    _context.SaveChanges();
                }

                // -------------------------------------------------
                // 4. ÜCRET DURUMU
                // -------------------------------------------------
                var ucretDurumu = new OgrenciUcretDurumu
                {
                    OgrenciKaydiId = ogrenciKaydi.Id,
                    Indirimli = model.Indirimli,
                    Ucretsiz = model.Ucretsiz,
                    IndirimOrani = model.Indirimli
                        ? model.IndirimOrani
                        : 0,
                    OzelFiyat = model.OzelFiyat,
                    Aciklama = model.UcretAciklama
                };

                _context.OgrenciUcretDurumlari.Add(ucretDurumu);
                _context.SaveChanges();

                // -------------------------------------------------
                // 5. ÖZEL ÜCRET
                // -------------------------------------------------
                if (model.OzelFiyat.HasValue &&
                    model.OzelFiyat.Value > 0)
                {
                    var ozelUcret = new OgrenciOzelUcret
                    {
                        OgrenciKaydiId = ogrenciKaydi.Id,
                        OzelUcret = model.OzelFiyat.Value,
                        Aciklama = model.UcretAciklama,
                        OlusturmaTarihi = DateTime.Now,
                        GuncellemeTarihi = DateTime.Now
                    };

                    _context.OgrenciOzelUcretler.Add(ozelUcret);
                    _context.SaveChanges();
                }

                // -------------------------------------------------
                // 6. ÖDENECEK TOPLAM ÜCRETİ HESAPLA
                // -------------------------------------------------
                decimal uygulanacakUcret = 0;

                if (!model.Ucretsiz)
                {
                    if (model.OzelFiyat.HasValue &&
                        model.OzelFiyat.Value > 0)
                    {
                        uygulanacakUcret = model.OzelFiyat.Value;
                    }
                    else if (okulUcreti != null)
                    {
                        uygulanacakUcret =
                            okulUcreti.UygulanacakStandartVeliUcreti;
                    }

                    if (model.Indirimli &&
                        model.IndirimOrani > 0)
                    {
                        uygulanacakUcret = Math.Round(
                            uygulanacakUcret *
                            (1 - model.IndirimOrani / 100m),
                            2);
                    }
                }

                // -------------------------------------------------
                // 7. AYLIK BORÇLAR
                // Okulun açık ayları: Ekim-Mayıs
                // -------------------------------------------------
                if (!model.Ucretsiz && uygulanacakUcret > 0)
                {
                    var aylar = new List<(int Ay, int Yil)>();
                    var bugun = DateTime.Today.Year;

                    var acikAylar = new[]
                    {
                        10, 11, 12, 1, 2, 3, 4, 5
                    };

                    foreach (var ay in acikAylar)
                    {
                        var yil = ay >= 10
                            ? bugun
                            : bugun + 1;

                        aylar.Add((ay, yil));
                    }

                    int taksitSayisi = model.OdemeSekli == "Pesin"
                        ? 1
                        : Math.Min(model.TaksitSayisi, aylar.Count);

                    var secilenAylar = aylar
                        .Skip(Math.Max(0, model.BaslangicAyi == 10 ? 0 : 0))
                        .Take(taksitSayisi)
                        .ToList();

                    var aylikTutar = Math.Round(
                        uygulanacakUcret / taksitSayisi,
                        2);

                    var borclar = new List<AylikBorc>();

                    for (var i = 0; i < secilenAylar.Count; i++)
                    {
                        var (ay, yil) = secilenAylar[i];

                        var tutar = i == secilenAylar.Count - 1
                            ? uygulanacakUcret -
                              (aylikTutar * (secilenAylar.Count - 1))
                            : aylikTutar;

                        var odemeTarihi = new DateTime(
                            yil,
                            ay,
                            Math.Min(model.OdemeGunu, DateTime.DaysInMonth(yil, ay))
                        );

                        borclar.Add(new AylikBorc
                        {
                            OgrenciKaydiId = ogrenciKaydi.Id,
                            Yil = yil,
                            Ay = ay,
                            OdemeTarihi = odemeTarihi,
                            Tutar = tutar,
                            OdenenTutar = 0,
                            Aciklama = ""
                        });
                    }

                    _context.AylikBorclar.AddRange(borclar);
                    _context.SaveChanges();
                }

                transaction.Commit();

                TempData["Basarili"] =
                    $"{ogrenci.AdSoyad} öğrencisinin kaydı başarıyla oluşturuldu.";

                return RedirectToAction(nameof(Kayit));
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                Console.WriteLine("================================");
                Console.WriteLine("BİRLEŞİK ÖĞRENCİ KAYDI HATASI");
                Console.WriteLine(ex);
                Console.WriteLine("================================");

                ModelState.AddModelError(
                    "",
                    "Kayıt sırasında bir hata oluştu. İşlem geri alındı.");

                return View("Kayit", model);
            }
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
                return NotFound();

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 1. Öğrenciye ait yoklama detaylarını sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM YoklamaDetaylari
                    WHERE OgrenciId = {id}
                    """);

                // 2. Öğrencinin öğrenci kayıtlarını al
                var kayitIds = _context.OgrenciKayitlari
                    .Where(x => x.OgrenciId == id)
                    .Select(x => x.Id)
                    .ToList();

                if (kayitIds.Any())
                {
                    // 3. Bu öğrenci kayıtlarına bağlı borçların ödeme dağılımlarını sil
                    foreach (var kayitId in kayitIds)
                    {
                        _context.Database.ExecuteSqlInterpolated($"""
                            DELETE FROM OdemeDagilimlari
                            WHERE AylikBorcId IN (
                                SELECT Id
                                FROM AylikBorclar
                                WHERE OgrenciKaydiId = {kayitId}
                            )
                            """);

                        // 4. Öğrenci kayıtlarına bağlı özel ücretleri sil
                        _context.Database.ExecuteSqlInterpolated($"""
                            DELETE FROM OgrenciOzelUcretler
                            WHERE OgrenciKaydiId = {kayitId}
                            """);

                        // 5. Öğrenci kayıtlarına bağlı aylık borçları sil
                        _context.Database.ExecuteSqlInterpolated($"""
                            DELETE FROM AylikBorclar
                            WHERE OgrenciKaydiId = {kayitId}
                            """);

                        // 6. Öğrenci kayıtlarına bağlı ücret durumlarını sil
                        _context.Database.ExecuteSqlInterpolated($"""
                            DELETE FROM OgrenciUcretDurumlari
                            WHERE OgrenciKaydiId = {kayitId}
                            """);
                    }
                }

                // 7. Öğrenciye ait ödemelerin ödeme dağılımlarını sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM OdemeDagilimlari
                    WHERE OdemeId IN (
                        SELECT Id
                        FROM Odemeler
                        WHERE OgrenciId = {id}
                    )
                    """);

                // 8. Öğrenciye ait ödemeleri sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM Odemeler
                    WHERE OgrenciId = {id}
                    """);

                // 9. Öğrenci-okul kayıtlarını sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM OgrenciKayitlari
                    WHERE OgrenciId = {id}
                    """);

                // 10. Öğrenci-veli bağlantılarını sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM OgrenciVeliler
                    WHERE OgrenciId = {id}
                    """);

                // 11. Ana öğrenciyi sil
                _context.Database.ExecuteSqlInterpolated($"""
                    DELETE FROM Ogrenciler
                    WHERE Id = {id}
                    """);

                transaction.Commit();

                TempData["DeleteSuccess"] =
                    ogrenci.AdSoyad + " öğrencisi ve kendisine bağlı kayıtlar başarıyla silindi.";
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                Console.WriteLine("================================");
                Console.WriteLine("ÖĞRENCİ SİLME HATASI");
                Console.WriteLine("ÖĞRENCİ ID: " + id);
                Console.WriteLine("HATA: " + ex.Message);
                Console.WriteLine("================================");

                TempData["DeleteError"] =
                    ogrenci.AdSoyad + " öğrencisi silinemedi. İşlem geri alındı.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}