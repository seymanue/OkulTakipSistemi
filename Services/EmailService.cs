using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace OkulTakipSistemi.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task MailGonderAsync(
            string aliciEmail,
            string konu,
            string mesaj)
        {
            var emailAyar = _configuration
                .GetSection("EmailSettings");

            var smtpServer =
                emailAyar["SmtpServer"] ?? "smtp.gmail.com";

            var smtpPort =
                int.Parse(emailAyar["SmtpPort"] ?? "587");

            var gonderenEmail =
                emailAyar["SenderEmail"];

            var gonderenSifre =
                emailAyar["AppPassword"];

            var gonderenAd =
                emailAyar["SenderName"] ?? "Okul Takip Sistemi";

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("EMAIL GÖNDERME BAŞLADI");
            Console.WriteLine("========================================");
            Console.WriteLine($"SMTP Server : {smtpServer}");
            Console.WriteLine($"SMTP Port   : {smtpPort}");
            Console.WriteLine($"Gönderen    : {gonderenEmail}");
            Console.WriteLine($"Alıcı       : {aliciEmail}");
            Console.WriteLine($"Konu        : {konu}");
            Console.WriteLine("========================================");

            if (string.IsNullOrWhiteSpace(gonderenEmail))
            {
                throw new Exception(
                    "SenderEmail ayarı bulunamadı.");
            }

            if (string.IsNullOrWhiteSpace(gonderenSifre))
            {
                throw new Exception(
                    "AppPassword ayarı bulunamadı.");
            }

            // --------------------------------------------------------
            // EMAIL OLUŞTUR
            // --------------------------------------------------------

            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    gonderenAd,
                    gonderenEmail));

            email.To.Add(
                MailboxAddress.Parse(aliciEmail));

            email.Subject = konu;

            email.Body = new TextPart("plain")
            {
                Text = mesaj
            };

            // --------------------------------------------------------
            // SMTP BAĞLANTISI
            // --------------------------------------------------------

            using var smtp = new SmtpClient();

            smtp.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) =>
                    true;

            Console.WriteLine("SMTP sunucusuna bağlanılıyor...");

            await smtp.ConnectAsync(
                smtpServer,
                smtpPort,
                SecureSocketOptions.StartTls);

            Console.WriteLine("SMTP bağlantısı başarılı.");

            // --------------------------------------------------------
            // GMAIL GİRİŞ
            // --------------------------------------------------------

            Console.WriteLine("Gmail hesabına giriş yapılıyor...");

            await smtp.AuthenticateAsync(
                gonderenEmail,
                gonderenSifre);

            Console.WriteLine("Gmail kimlik doğrulaması başarılı.");

            // --------------------------------------------------------
            // EMAIL GÖNDER
            // --------------------------------------------------------

            Console.WriteLine("Email gönderiliyor...");

            await smtp.SendAsync(email);

            Console.WriteLine("EMAIL BAŞARIYLA GÖNDERİLDİ.");

            // --------------------------------------------------------
            // BAĞLANTIYI KAPAT
            // --------------------------------------------------------

            await smtp.DisconnectAsync(true);

            Console.WriteLine("SMTP bağlantısı kapatıldı.");
            Console.WriteLine("========================================");
        }
    }
}