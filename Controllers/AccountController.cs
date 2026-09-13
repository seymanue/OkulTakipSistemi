using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Models;
using OkulTakipSistemi.Services;

namespace OkulTakipSistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public AccountController(
            AppDbContext context,
            EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ============================================================
        // ÖĞRETMEN GİRİŞ SAYFASI
        // ============================================================

        [HttpGet]
        public IActionResult Login()
        {
            var hatirlananKullanici = Request.Cookies["HatirlananKullanici"];

            if (!string.IsNullOrWhiteSpace(hatirlananKullanici))
            {
                ViewBag.HatirlananKullanici = hatirlananKullanici;
            }

            return View();
        }

        // ============================================================
        // YÖNETİCİ GİRİŞ SAYFASI
        // ============================================================

        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View();
        }

        // ============================================================
        // GİRİŞ İŞLEMİ
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(
            string KullaniciAdi,
            string Sifre,
            string GirisTuru,
            bool BeniHatirla)
        {
            if (string.IsNullOrWhiteSpace(GirisTuru))
            {
                GirisTuru = "Ogretmen";
            }

            var kullanici = _context.Kullanicilar
                .Include(x => x.Rol)
                .FirstOrDefault(x =>
                    x.KullaniciAdi == KullaniciAdi &&
                    x.Sifre == Sifre);

            if (kullanici == null)
            {
                ViewBag.Hata =
                    "Kullanıcı adı veya şifre hatalı.";

                if (GirisTuru == "Admin")
                {
                    return View("AdminLogin");
                }

                return View("Login");
            }

            // ========================================================
            // YÖNETİCİ GİRİŞİ
            // ========================================================

            if (GirisTuru == "Admin")
            {
                if (kullanici.Rol?.Ad != "Admin")
                {
                    ViewBag.Hata =
                        "Bu hesap yönetici olarak giriş yapamaz.";

                    return View("AdminLogin");
                }

                OturumAc(kullanici);

                return RedirectToAction(
                    "AdminPanel",
                    "Home");
            }

            // ========================================================
            // ÖĞRETMEN GİRİŞİ
            // ========================================================

            if (GirisTuru == "Ogretmen")
            {
                if (kullanici.Rol?.Ad != "Öğretmen")
                {
                    ViewBag.Hata =
                        "Bu hesap öğretmen olarak giriş yapamaz.";

                    return View("Login");
                }

                if (BeniHatirla)
                {
                    Response.Cookies.Append(
                        "HatirlananKullanici",
                        kullanici.KullaniciAdi,
                        new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddDays(30),
                            HttpOnly = true,
                            IsEssential = true,
                            Secure = Request.IsHttps
                        });
                }
                else
                {
                    Response.Cookies.Delete("HatirlananKullanici");
                }

                OturumAc(kullanici);

                return RedirectToAction(
                    "OgretmenPanel",
                    "Home");
            }

            ViewBag.Hata =
                "Geçersiz giriş türü.";

            return View("Login");
        }

        // ============================================================
        // OTURUM AÇ
        // ============================================================

        private void OturumAc(Kullanici kullanici)
        {
            HttpContext.Session.SetInt32(
                "KullaniciId",
                kullanici.Id);

            HttpContext.Session.SetString(
                "KullaniciAdi",
                kullanici.KullaniciAdi);

            HttpContext.Session.SetString(
                "AdSoyad",
                kullanici.AdSoyad);

            HttpContext.Session.SetString(
                "Rol",
                kullanici.Rol?.Ad ?? "");

            HttpContext.Session.SetString(
                "Email",
                kullanici.Email ?? "");
        }

        // ============================================================
        // ÖĞRETMEN KAYIT SAYFASI
        // ============================================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ============================================================
        // ÖĞRETMEN KAYIT İŞLEMİ
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (model.Sifre != model.SifreTekrar)
            {
                ModelState.AddModelError(
                    "SifreTekrar",
                    "Şifreler aynı değil.");
            }

            var mevcutKullanici = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.KullaniciAdi == model.KullaniciAdi);

            if (mevcutKullanici != null)
            {
                ModelState.AddModelError(
                    "KullaniciAdi",
                    "Bu kullanıcı adı zaten kullanılıyor.");
            }

            var mevcutEmail = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.Email == model.Email);

            if (mevcutEmail != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "Bu email adresi zaten kullanılıyor.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var ogretmenRol = _context.Roller
                .FirstOrDefault(x => x.Ad == "Öğretmen");

            if (ogretmenRol == null)
            {
                ModelState.AddModelError(
                    "",
                    "Öğretmen rolü bulunamadı.");

                return View(model);
            }

            var kullanici = new Kullanici
            {
                AdSoyad = model.AdSoyad,
                KullaniciAdi = model.KullaniciAdi,
                Email = model.Email,
                Sifre = model.Sifre,
                RolId = ogretmenRol.Id
            };

            _context.Kullanicilar.Add(kullanici);
            _context.SaveChanges();

            TempData["Basari"] =
                "Öğretmen hesabı başarıyla oluşturuldu.";

            return RedirectToAction("Login");
        }

        // ============================================================
        // YÖNETİCİ KAYIT SAYFASI
        // ============================================================

        [HttpGet]
        public IActionResult AdminRegister()
        {
            return View();
        }

        // ============================================================
        // YÖNETİCİ KAYIT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminRegister(
            RegisterViewModel model)
        {
            if (model.Sifre != model.SifreTekrar)
            {
                ModelState.AddModelError(
                    "SifreTekrar",
                    "Şifreler aynı değil.");
            }

            var mevcutKullanici = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.KullaniciAdi == model.KullaniciAdi);

            if (mevcutKullanici != null)
            {
                ModelState.AddModelError(
                    "KullaniciAdi",
                    "Bu kullanıcı adı zaten kullanılıyor.");
            }

            var mevcutEmail = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.Email == model.Email);

            if (mevcutEmail != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "Bu email adresi zaten kullanılıyor.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminRol = _context.Roller
                .FirstOrDefault(x => x.Ad == "Admin");

            if (adminRol == null)
            {
                ModelState.AddModelError(
                    "",
                    "Admin rolü bulunamadı.");

                return View(model);
            }

            var kod = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();

            HttpContext.Session.SetString(
                "AdminRegister_AdSoyad",
                model.AdSoyad);

            HttpContext.Session.SetString(
                "AdminRegister_KullaniciAdi",
                model.KullaniciAdi);

            HttpContext.Session.SetString(
                "AdminRegister_Email",
                model.Email);

            HttpContext.Session.SetString(
                "AdminRegister_Sifre",
                model.Sifre);

            HttpContext.Session.SetInt32(
                "AdminRegister_RolId",
                adminRol.Id);

            HttpContext.Session.SetString(
                "AdminRegister_DogrulamaKodu",
                kod);

            try
            {
                await _emailService.MailGonderAsync(
                    model.Email,
                    "Okul Takip Sistemi - Yönetici Email Doğrulama Kodu",
                    $"""
                    Merhaba {model.AdSoyad},

                    Yönetici hesabınızı oluşturmak için
                    doğrulama kodunuz:

                    {kod}

                    Eğer bu işlemi siz başlatmadıysanız,
                    bu emaili dikkate almayabilirsiniz.

                    Okul Takip Sistemi
                    """);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("ADMIN EMAIL GÖNDERME HATASI");
                Console.WriteLine("========================================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("========================================");

                ViewBag.Hata =
                    "Doğrulama emaili gönderilemedi: " +
                    ex.Message;

                return View(model);
            }

            return RedirectToAction(
                "AdminEmailDogrula");
        }

        // ============================================================
        // YÖNETİCİ EMAIL DOĞRULAMA SAYFASI
        // ============================================================

        [HttpGet]
        public IActionResult AdminEmailDogrula()
        {
            return View();
        }

        // ============================================================
        // YÖNETİCİ EMAIL DOĞRULAMA
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminEmailDogrula(
            string Kod)
        {
            var dogruKod =
                HttpContext.Session.GetString(
                    "AdminRegister_DogrulamaKodu");

            if (string.IsNullOrWhiteSpace(dogruKod))
            {
                ViewBag.Hata =
                    "Doğrulama kodu bulunamadı. Lütfen yeniden kayıt olun.";

                return View();
            }

            if (string.IsNullOrWhiteSpace(Kod) ||
                Kod.Trim() != dogruKod)
            {
                ViewBag.Hata =
                    "Doğrulama kodu yanlış.";

                return View();
            }

            var adSoyad =
                HttpContext.Session.GetString(
                    "AdminRegister_AdSoyad");

            var kullaniciAdi =
                HttpContext.Session.GetString(
                    "AdminRegister_KullaniciAdi");

            var email =
                HttpContext.Session.GetString(
                    "AdminRegister_Email");

            var sifre =
                HttpContext.Session.GetString(
                    "AdminRegister_Sifre");

            var rolId =
                HttpContext.Session.GetInt32(
                    "AdminRegister_RolId");

            if (string.IsNullOrWhiteSpace(adSoyad) ||
                string.IsNullOrWhiteSpace(kullaniciAdi) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(sifre) ||
                rolId == null)
            {
                ViewBag.Hata =
                    "Kayıt bilgileri bulunamadı. Lütfen yeniden kayıt olun.";

                return View();
            }

            var kullanici = new Kullanici
            {
                AdSoyad = adSoyad,
                KullaniciAdi = kullaniciAdi,
                Email = email,
                Sifre = sifre,
                RolId = rolId.Value
            };

            _context.Kullanicilar.Add(kullanici);
            _context.SaveChanges();

            HttpContext.Session.Remove(
                "AdminRegister_AdSoyad");

            HttpContext.Session.Remove(
                "AdminRegister_KullaniciAdi");

            HttpContext.Session.Remove(
                "AdminRegister_Email");

            HttpContext.Session.Remove(
                "AdminRegister_Sifre");

            HttpContext.Session.Remove(
                "AdminRegister_RolId");

            HttpContext.Session.Remove(
                "AdminRegister_DogrulamaKodu");

            TempData["Basari"] =
                "Email adresiniz doğrulandı. Yönetici hesabınız başarıyla oluşturuldu.";

            return RedirectToAction(
                "AdminLogin");
        }

        // ============================================================
        // KULLANICI BİLGİLERİ SAYFASI
        // ============================================================

        [HttpGet]
        public IActionResult OgretmenBilgileri()
        {
            var kullaniciId =
                HttpContext.Session.GetInt32(
                    "KullaniciId");

            if (kullaniciId == null)
            {
                return RedirectToAction(
                    "Login");
            }

            var kullanici = _context.Kullanicilar
                .Include(x => x.Rol)
                .FirstOrDefault(x =>
                    x.Id == kullaniciId.Value);

            if (kullanici == null)
            {
                HttpContext.Session.Clear();

                return RedirectToAction(
                    "Login");
            }

            // Öğretmen VE Admin kendi bilgilerine erişebilir.
            if (kullanici.Rol?.Ad != "Öğretmen" &&
                kullanici.Rol?.Ad != "Admin")
            {
                return RedirectToAction(
                    "Login");
            }

            var model = new OgretmenBilgileriViewModel
            {
                AdSoyad = kullanici.AdSoyad,
                KullaniciAdi = kullanici.KullaniciAdi,
                Email = kullanici.Email
            };

            return View(model);
        }

        // ============================================================
        // KULLANICI BİLGİLERİ GÜNCELLEME
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OgretmenBilgileri(
            OgretmenBilgileriViewModel model)
        {
            var kullaniciId =
                HttpContext.Session.GetInt32(
                    "KullaniciId");

            if (kullaniciId == null)
            {
                return RedirectToAction(
                    "Login");
            }

            var kullanici = _context.Kullanicilar
                .Include(x => x.Rol)
                .FirstOrDefault(x =>
                    x.Id == kullaniciId.Value);

            if (kullanici == null)
            {
                HttpContext.Session.Clear();

                return RedirectToAction(
                    "Login");
            }

            // Öğretmen VE Admin kendi bilgilerini güncelleyebilir.
            if (kullanici.Rol?.Ad != "Öğretmen" &&
                kullanici.Rol?.Ad != "Admin")
            {
                return RedirectToAction(
                    "Login");
            }

            // ========================================================
            // AD SOYAD KONTROLÜ
            // ========================================================

            if (string.IsNullOrWhiteSpace(model.AdSoyad))
            {
                ModelState.AddModelError(
                    "AdSoyad",
                    "Ad Soyad boş bırakılamaz.");
            }

            // ========================================================
            // KULLANICI ADI KONTROLÜ
            // ========================================================

            if (string.IsNullOrWhiteSpace(model.KullaniciAdi))
            {
                ModelState.AddModelError(
                    "KullaniciAdi",
                    "Kullanıcı adı boş bırakılamaz.");
            }

            // ========================================================
            // EMAIL KONTROLÜ
            // ========================================================

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(
                    "Email",
                    "E-posta boş bırakılamaz.");
            }

            // ========================================================
            // KULLANICI ADI BAŞKA KULLANICIDA VAR MI?
            // ========================================================

            var kullaniciAdiKontrol =
                _context.Kullanicilar
                    .FirstOrDefault(x =>
                        x.KullaniciAdi == model.KullaniciAdi &&
                        x.Id != kullanici.Id);

            if (kullaniciAdiKontrol != null)
            {
                ModelState.AddModelError(
                    "KullaniciAdi",
                    "Bu kullanıcı adı zaten kullanılıyor.");
            }

            // ========================================================
            // EMAIL BAŞKA KULLANICIDA VAR MI?
            // ========================================================

            var emailKontrol =
                _context.Kullanicilar
                    .FirstOrDefault(x =>
                        x.Email == model.Email &&
                        x.Id != kullanici.Id);

            if (emailKontrol != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "Bu email adresi zaten kullanılıyor.");
            }

            // ========================================================
            // HATALIYSA FORMU GERİ GÖSTER
            // ========================================================

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ========================================================
            // VERİTABANINDAKİ GERÇEK KAYDI GÜNCELLE
            // ========================================================

            kullanici.AdSoyad =
                model.AdSoyad.Trim();

            kullanici.KullaniciAdi =
                model.KullaniciAdi.Trim();

            kullanici.Email =
                model.Email.Trim();

            _context.SaveChanges();

            // ========================================================
            // SESSION BİLGİLERİNİ GÜNCELLE
            // ========================================================

            HttpContext.Session.SetString(
                "AdSoyad",
                kullanici.AdSoyad);

            HttpContext.Session.SetString(
                "KullaniciAdi",
                kullanici.KullaniciAdi);

            HttpContext.Session.SetString(
                "Email",
                kullanici.Email);

            TempData["Basari"] =
                "Bilgileriniz başarıyla güncellendi.";

            return RedirectToAction(
                "OgretmenBilgileri");
        }

        // ============================================================
        // ŞİFREMİ UNUTTUM - SAYFA
        // ============================================================

        [HttpGet]
        public IActionResult SifremiUnuttum(
            string GirisTuru = "Ogretmen")
        {
            ViewBag.GirisTuru =
                GirisTuru;

            return View();
        }

        // ============================================================
        // ŞİFREMİ UNUTTUM - İŞLEM
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SifremiUnuttum(
            string KullaniciAdi,
            string YeniSifre,
            string YeniSifreTekrar,
            string GirisTuru)
        {
            if (string.IsNullOrWhiteSpace(GirisTuru))
            {
                GirisTuru = "Ogretmen";
            }

            ViewBag.GirisTuru =
                GirisTuru;

            if (string.IsNullOrWhiteSpace(KullaniciAdi))
            {
                ViewBag.Hata =
                    "Kullanıcı adı boş bırakılamaz.";

                return View();
            }

            if (string.IsNullOrWhiteSpace(YeniSifre))
            {
                ViewBag.Hata =
                    "Yeni şifre boş bırakılamaz.";

                return View();
            }

            if (YeniSifre != YeniSifreTekrar)
            {
                ViewBag.Hata =
                    "Şifreler aynı değil.";

                return View();
            }

            var kullanici = _context.Kullanicilar
                .Include(x => x.Rol)
                .FirstOrDefault(x =>
                    x.KullaniciAdi == KullaniciAdi);

            if (kullanici == null)
            {
                ViewBag.Hata =
                    "Bu kullanıcı adına ait hesap bulunamadı.";

                return View();
            }

            if (GirisTuru == "Admin")
            {
                if (kullanici.Rol?.Ad != "Admin")
                {
                    ViewBag.Hata =
                        "Bu hesap yönetici hesabı değil.";

                    return View();
                }
            }

            if (GirisTuru == "Ogretmen")
            {
                if (kullanici.Rol?.Ad != "Öğretmen")
                {
                    ViewBag.Hata =
                        "Bu hesap öğretmen hesabı değil.";

                    return View();
                }
            }

            kullanici.Sifre =
                YeniSifre;

            _context.SaveChanges();

            TempData["Basari"] =
                "Şifreniz başarıyla değiştirildi.";

            if (GirisTuru == "Admin")
            {
                return RedirectToAction(
                    "AdminLogin");
            }

            return RedirectToAction(
                "Login");
        }

        // ============================================================
        // ÇIKIŞ
        // ============================================================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                "Login");
        }
    }
}