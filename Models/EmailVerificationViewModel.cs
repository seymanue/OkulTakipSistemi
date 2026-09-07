using System.ComponentModel.DataAnnotations;

namespace OkulTakipSistemi.Models
{
    public class EmailVerificationViewModel
    {
        [Required(ErrorMessage = "Doğrulama kodu zorunludur.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Kod 6 haneli olmalıdır.")]
        public string Kod { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}