using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Öğrenci numarası (Email) zorunludur.")]
        [EmailAddress]
        [Display(Name = "Email Adresiniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}