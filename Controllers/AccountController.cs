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
                    x.Sifre == Sifre &&
                    (
                        (GirisTuru == "Admin" && x.Rol.Ad == "Admin") ||
                        (GirisTuru == "Ogretmen" && x.Rol.Ad == "Öğretmen")
                    ));

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
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.Sifre != model.SifreTekrar)
            {
                ModelState.AddModelError(
                    "SifreTekrar",
                    "Şifreler aynı değil.");
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

            var mevcutKullanici = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.KullaniciAdi == model.KullaniciAdi &&
                    x.RolId == ogretmenRol.Id);

            if (mevcutKullanici != null)
            {
                ModelState.AddModelError(
                    "KullaniciAdi",
                    "Bu kullanıcı adı zaten öğretmen hesabında kullanılıyor.");
            }

            var mevcutEmail = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.Email == model.Email &&
                    x.RolId == ogretmenRol.Id);

            if (mevcutEmail != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "Bu email adresi zaten öğretmen hesabında kullanılıyor.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var kod = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();

            HttpContext.Session.SetString(
                "TeacherRegister_AdSoyad",
                model.AdSoyad);

            HttpContext.Session.SetString(
                "TeacherRegister_KullaniciAdi",
                model.KullaniciAdi);

            HttpContext.Session.SetString(
                "TeacherRegister_Email",
                model.Email);

            HttpContext.Session.SetString(
                "TeacherRegister_Sifre",
                model.Sifre);

            HttpContext.Session.SetInt32(
                "TeacherRegister_RolId",
                ogretmenRol.Id);

            HttpContext.Session.SetString(
                "TeacherRegister_DogrulamaKodu",
                kod);

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("ADMIN EMAIL GÖNDERİLİYOR");
            Console.WriteLine("Alıcı: " + model.Email);
            Console.WriteLine("Doğrulama Kodu: " + kod);
            Console.WriteLine("========================================");

            try
            {
                await _emailService.MailGonderAsync(
                    model.Email,
                    "Okul Takip Sistemi - Öğretmen Email Doğrulama Kodu",
                    $"""
                    Merhaba {model.AdSoyad},

                    Öğretmen hesabınızı oluşturmak için
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
                Console.WriteLine("ÖĞRETMEN EMAIL GÖNDERME HATASI");
                Console.WriteLine("========================================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("========================================");

                ViewBag.Hata =
                    "Doğrulama emaili gönderilemedi: " +
                    ex.Message;

                return View(model);
            }

            return RedirectToAction(
                "EmailDogrula");
        }

        // ============================================================
        // YÖNETİCİ KAYIT SAYFASI
        // ============================================================

        [HttpGet]
        public IActionResult AdminRegister()
        {
            var dogrulamaKodu =
                HttpContext.Session.GetString(
                    "AdminRegister_DogrulamaKodu");

            ViewBag.ShowVerification =
                !string.IsNullOrWhiteSpace(dogrulamaKodu);

            return View();
        }

        // ============================================================
        // YÖNETİCİ KAYIT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminRegister(
            RegisterViewModel model,
            string? Kod)
        {
            // ========================================================
            // KOD DOĞRULAMA
            // ========================================================

            if (!string.IsNullOrWhiteSpace(Kod))
            {
                var dogruKod =
                    HttpContext.Session.GetString(
                        "AdminRegister_DogrulamaKodu");

                if (string.IsNullOrWhiteSpace(dogruKod))
                {
                    ViewBag.Hata =
                        "Doğrulama kodu bulunamadı. Lütfen yeniden kayıt olun.";

                    ViewBag.ShowVerification = false;

                    return View(model);
                }

                if (Kod.Trim() != dogruKod)
                {
                    ViewBag.Hata =
                        "Doğrulama kodu yanlış.";

                    ViewBag.ShowVerification = true;

                    return View(model);
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

                    ViewBag.ShowVerification = false;

                    return View(model);
                }

                var kullanici = new Kullanici
                {
                    AdSoyad = adSoyad,
                    KullaniciAdi = kullaniciAdi,
                    Email = email,
                    Sifre = sifre,
                    RolId = rolId.Value,
                    EmailDogrulandi = true
                };

                _context.Kullanicilar.Add(kullanici);
                _context.SaveChanges();

                HttpContext.Session.Remove("AdminRegister_AdSoyad");
                HttpContext.Session.Remove("AdminRegister_KullaniciAdi");
                HttpContext.Session.Remove("AdminRegister_Email");
                HttpContext.Session.Remove("AdminRegister_Sifre");
                HttpContext.Session.Remove("AdminRegister_RolId");
                HttpContext.Session.Remove("AdminRegister_DogrulamaKodu");

                TempData["Basari"] =
                    "Hesabınız başarıyla oluşturuldu.";

                return RedirectToAction("AdminLogin");
            }

            // ========================================================
            // YENİ YÖNETİCİ KAYDI
            // ========================================================

            if (model.Sifre != model.SifreTekrar)
            {
                ModelState.AddModelError(
                    "SifreTekrar",
                    "Şifreler aynı değil.");
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

            // Aynı kullanıcı adı yalnızca ADMIN hesapları arasında kontrol edilir.
            var mevcutKullanici = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.KullaniciAdi == model.KullaniciAdi &&
                    x.RolId == adminRol.Id);

            if (mevcutKullanici != null)
            {
                ModelState.AddModelError(
                    "KullaniciAdi",
                    "Bu kullanıcı adı zaten yönetici hesabında kullanılıyor.");
            }

            // Aynı email yalnızca ADMIN hesapları arasında kontrol edilir.
            var mevcutEmail = _context.Kullanicilar
                .FirstOrDefault(x =>
                    x.Email == model.Email &&
                    x.RolId == adminRol.Id);

            if (mevcutEmail != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "Bu email adresi zaten yönetici hesabında kullanılıyor.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ShowVerification = false;
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
                Console.WriteLine("YÖNETİCİ EMAIL GÖNDERME HATASI");
                Console.WriteLine("========================================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("========================================");

                ViewBag.Hata =
                    "Doğrulama emaili gönderilemedi: " +
                    ex.Message;

                HttpContext.Session.Remove("AdminRegister_AdSoyad");
                HttpContext.Session.Remove("AdminRegister_KullaniciAdi");
                HttpContext.Session.Remove("AdminRegister_Email");
                HttpContext.Session.Remove("AdminRegister_Sifre");
                HttpContext.Session.Remove("AdminRegister_RolId");
                HttpContext.Session.Remove("AdminRegister_DogrulamaKodu");

                ViewBag.ShowVerification = false;

                return View(model);
            }

            ViewBag.ShowVerification = true;

            return View(model);
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
                "Hesabınız başarıyla oluşturuldu.";

            return RedirectToAction(
                "AdminLogin");
        }

        // ============================================================
        // ÖĞRETMEN EMAIL DOĞRULAMA
        // ============================================================

        [HttpGet]
        public IActionResult EmailDogrula()
        {
            return View();
        }

        // ============================================================
        // ÖĞRETMEN EMAIL DOĞRULAMA İŞLEMİ
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EmailDogrula(string Kod)
        {
            var dogruKod =
                HttpContext.Session.GetString(
                    "TeacherRegister_DogrulamaKodu");

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
                    "TeacherRegister_AdSoyad");

            var kullaniciAdi =
                HttpContext.Session.GetString(
                    "TeacherRegister_KullaniciAdi");

            var email =
                HttpContext.Session.GetString(
                    "TeacherRegister_Email");

            var sifre =
                HttpContext.Session.GetString(
                    "TeacherRegister_Sifre");

            var rolId =
                HttpContext.Session.GetInt32(
                    "TeacherRegister_RolId");

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

            var ogretmen = new Ogretmen
            {
                AdSoyad = adSoyad,
                KullaniciId = kullanici.Id
            };

            _context.Ogretmenler.Add(ogretmen);
            _context.SaveChanges();

            HttpContext.Session.Remove(
                "TeacherRegister_AdSoyad");

            HttpContext.Session.Remove(
                "TeacherRegister_KullaniciAdi");

            HttpContext.Session.Remove(
                "TeacherRegister_Email");

            HttpContext.Session.Remove(
                "TeacherRegister_Sifre");

            HttpContext.Session.Remove(
                "TeacherRegister_RolId");

            HttpContext.Session.Remove(
                "TeacherRegister_DogrulamaKodu");

            TempData["Basari"] =
                "Hesabınız başarıyla oluşturuldu.";

            return RedirectToAction("Login");
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
        // ŞİFREMİ UNUTTUM - TEK EKRAN
        // ============================================================

        [HttpGet]
        public IActionResult SifremiUnuttum(
            string GirisTuru = "Ogretmen")
        {
            ViewBag.GirisTuru = GirisTuru;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SifremiUnuttum(
            string Email,
            string Kod,
            string YeniSifre,
            string YeniSifreTekrar,
            string GirisTuru,
            string Adim = "Email")
        {
            if (string.IsNullOrWhiteSpace(GirisTuru))
            {
                GirisTuru = "Ogretmen";
            }

            ViewBag.GirisTuru = GirisTuru;

            // ========================================================
            // YENİ KOD GÖNDER
            // ========================================================

            if (Adim == "YeniKod")
            {
                var kullaniciId =
                    HttpContext.Session.GetInt32(
                        "PasswordReset_KullaniciId");

                var kayitliEmail =
                    HttpContext.Session.GetString(
                        "PasswordReset_Email");

                var kayitliGirisTuru =
                    HttpContext.Session.GetString(
                        "PasswordReset_GirisTuru")
                    ?? GirisTuru;

                if (kullaniciId == null ||
                    string.IsNullOrWhiteSpace(kayitliEmail))
                {
                    ViewBag.Hata =
                        "Şifre sıfırlama oturumu bulunamadı. Lütfen işlemi yeniden başlatınız.";

                    return View();
                }

                var kullanici =
                    _context.Kullanicilar
                        .Include(x => x.Rol)
                        .FirstOrDefault(x =>
                            x.Id == kullaniciId.Value);

                if (kullanici == null)
                {
                    ViewBag.Hata =
                        "Kullanıcı bulunamadı. Lütfen işlemi yeniden başlatınız.";

                    return View();
                }

                var yeniDogrulamaKodu =
                    RandomNumberGenerator
                        .GetInt32(100000, 1000000)
                        .ToString();

                HttpContext.Session.SetString(
                    "PasswordReset_Kod",
                    yeniDogrulamaKodu);

                HttpContext.Session.SetString(
                    "PasswordReset_KodOlusturmaZamani",
                    DateTimeOffset.UtcNow.ToString("O"));

                HttpContext.Session.Remove(
                    "PasswordReset_Dogrulandi");

                try
                {
                    await _emailService.MailGonderAsync(
                        kullanici.Email,
                        "Okul Takip Sistemi - Yeni Şifre Sıfırlama Kodu",
                        $"""
                        Merhaba {kullanici.AdSoyad},

                        Yeni doğrulama kodunuz:

                        {yeniDogrulamaKodu}

                        Bu kod 2 dakika boyunca geçerlidir.

                        Eğer bu işlemi siz başlatmadıysanız,
                        bu emaili dikkate almayabilirsiniz.

                        Okul Takip Sistemi
                        """);
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("========================================");
                    Console.WriteLine("YENİ ŞİFRE SIFIRLAMA EMAILİ GÖNDERME HATASI");
                    Console.WriteLine("========================================");
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("========================================");

                    ViewBag.Hata =
                        "Yeni doğrulama kodu gönderilemedi: " +
                        ex.Message;

                    return View();
                }

                ViewBag.GirisTuru = kayitliGirisTuru;
                ViewBag.KodGonderildi = true;

                return View();
            }

            // ========================================================
            // 1. AŞAMA - EMAIL KODU GÖNDER
            // ========================================================

            if (Adim == "Email")
            {
                if (string.IsNullOrWhiteSpace(Email))
                {
                    ViewBag.Hata =
                        "Email adresi boş bırakılamaz.";

                    return View();
                }

                var kullanici = _context.Kullanicilar
                    .Include(x => x.Rol)
                    .FirstOrDefault(x =>
                        x.Email == Email.Trim() &&
                        (
                            (GirisTuru == "Admin" && x.Rol!.Ad == "Admin") ||
                            (GirisTuru == "Ogretmen" && x.Rol!.Ad == "Öğretmen")
                        ));

                if (kullanici == null)
                {
                    ViewBag.Hata =
                        "Bu email adresine ait hesap bulunamadı.";

                    return View();
                }

                if (GirisTuru == "Admin" &&
                    kullanici.Rol?.Ad != "Admin")
                {
                    ViewBag.Hata =
                        "Bu hesap yönetici hesabı değil.";

                    return View();
                }

                if (GirisTuru == "Ogretmen" &&
                    kullanici.Rol?.Ad != "Öğretmen")
                {
                    ViewBag.Hata =
                        "Bu hesap öğretmen hesabı değil.";

                    return View();
                }

                if (string.IsNullOrWhiteSpace(kullanici.Email))
                {
                    ViewBag.Hata =
                        "Bu hesaba kayıtlı bir email adresi bulunamadı.";

                    return View();
                }

                var dogrulamaKodu =
                    RandomNumberGenerator
                        .GetInt32(100000, 1000000)
                        .ToString();

                HttpContext.Session.SetInt32(
                    "PasswordReset_KullaniciId",
                    kullanici.Id);

                HttpContext.Session.SetString(
                    "PasswordReset_Email",
                    kullanici.Email);

                HttpContext.Session.SetString(
                    "PasswordReset_Kod",
                    dogrulamaKodu);

                // Doğrulama kodu 2 dakika geçerlidir.
                HttpContext.Session.SetString(
                    "PasswordReset_KodOlusturmaZamani",
                    DateTimeOffset.UtcNow.ToString("O"));

                HttpContext.Session.SetString(
                    "PasswordReset_GirisTuru",
                    GirisTuru);

                try
                {
                    await _emailService.MailGonderAsync(
                        kullanici.Email,
                        "Okul Takip Sistemi - Şifre Sıfırlama Kodu",
                        $"""
                        Merhaba {kullanici.AdSoyad},

                        Şifrenizi yenilemek için doğrulama kodunuz:

                        {dogrulamaKodu}

                        Eğer bu işlemi siz başlatmadıysanız,
                        bu emaili dikkate almayabilirsiniz.

                        Okul Takip Sistemi
                        """);
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("========================================");
                    Console.WriteLine("ŞİFRE SIFIRLAMA EMAIL GÖNDERME HATASI");
                    Console.WriteLine("========================================");
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("========================================");

                    ViewBag.Hata =
                        "Doğrulama emaili gönderilemedi: " +
                        ex.Message;

                    return View();
                }

                ViewBag.KodGonderildi = true;

                return View();
            }

            // ========================================================
            // 2. AŞAMA - KOD DOĞRULAMA
            // ========================================================

            if (Adim == "Kod")
            {
                var dogruKod =
                    HttpContext.Session.GetString(
                        "PasswordReset_Kod");

                if (string.IsNullOrWhiteSpace(dogruKod))
                {
                    ViewBag.Hata =
                        "Doğrulama kodu bulunamadı. Lütfen yeniden kod isteyin.";

                    return View();
                }

                var kodOlusturmaZamaniString =
                    HttpContext.Session.GetString(
                        "PasswordReset_KodOlusturmaZamani");

                if (!DateTimeOffset.TryParse(
                        kodOlusturmaZamaniString,
                        out var kodOlusturmaZamani) ||
                    DateTimeOffset.UtcNow - kodOlusturmaZamani >
                    TimeSpan.FromMinutes(2))
                {
                    HttpContext.Session.Remove("PasswordReset_Kod");
                    HttpContext.Session.Remove("PasswordReset_KodOlusturmaZamani");

                    ViewBag.KodGonderildi = true;
                    ViewBag.KodSuresiDoldu = true;
                    ViewBag.Hata =
                        "Doğrulama kodunun süresi dolmuştur. Lütfen yeni bir kod isteyiniz.";

                    return View();
                }

                if (string.IsNullOrWhiteSpace(Kod) ||
                    Kod.Trim() != dogruKod)
                {
                    ViewBag.KodGonderildi = true;
                    ViewBag.Hata =
                        "Doğrulama kodu yanlış.";

                    return View();
                }

                HttpContext.Session.SetString(
                    "PasswordReset_Dogrulandi",
                    "true");

                ViewBag.KodDogrulandi = true;

                return View();
            }

            // ========================================================
            // 3. AŞAMA - YENİ ŞİFRE
            // ========================================================

            if (Adim == "Sifre")
            {
                var dogrulandi =
                    HttpContext.Session.GetString(
                        "PasswordReset_Dogrulandi");

                if (dogrulandi != "true")
                {
                    return RedirectToAction(
                        "SifremiUnuttum",
                        new
                        {
                            GirisTuru = GirisTuru
                        });
                }

                if (string.IsNullOrWhiteSpace(YeniSifre))
                {
                    ViewBag.KodDogrulandi = true;
                    ViewBag.Hata =
                        "Yeni şifre boş bırakılamaz.";

                    return View();
                }

                if (YeniSifre != YeniSifreTekrar)
                {
                    ViewBag.KodDogrulandi = true;
                    ViewBag.Hata =
                        "Şifreler aynı değil.";

                    return View();
                }

                var kullaniciId =
                    HttpContext.Session.GetInt32(
                        "PasswordReset_KullaniciId");

                if (kullaniciId == null)
                {
                    ViewBag.Hata =
                        "Kullanıcı bilgileri bulunamadı. Lütfen işlemi yeniden başlatın.";

                    return View();
                }

                var kullanici =
                    _context.Kullanicilar
                        .FirstOrDefault(x =>
                            x.Id == kullaniciId.Value);

                if (kullanici == null)
                {
                    ViewBag.Hata =
                        "Kullanıcı bulunamadı.";

                    return View();
                }

                kullanici.Sifre = YeniSifre;

                _context.SaveChanges();

                var sifirlananGirisTuru =
                    HttpContext.Session.GetString(
                        "PasswordReset_GirisTuru")
                    ?? GirisTuru;

                HttpContext.Session.Remove(
                    "PasswordReset_KullaniciId");

                HttpContext.Session.Remove(
                    "PasswordReset_Email");

                HttpContext.Session.Remove(
                    "PasswordReset_Kod");

                HttpContext.Session.Remove(
                    "PasswordReset_KodOlusturmaZamani");

                HttpContext.Session.Remove(
                    "PasswordReset_GirisTuru");

                HttpContext.Session.Remove(
                    "PasswordReset_Dogrulandi");

                TempData["Basari"] =
                    "Şifreniz başarıyla değiştirildi.";

                if (sifirlananGirisTuru == "Admin")
                {
                    return RedirectToAction(
                        "AdminLogin");
                }

                return RedirectToAction(
                    "Login");
            }

            return View();
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

        [HttpGet]
        public IActionResult AdminLogout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                "AdminLogin");
        }
    }
}