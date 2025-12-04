using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SporSalonuYonetim.Web.Models;

namespace SporSalonuYonetim.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // 1. Giriş Sayfasını Göster (GET)
        public IActionResult Login()
        {
            return View();
        }

        // 2. Giriş İşlemini Yap (POST)
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kullanıcıyı bul
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Bu email ile kayıtlı kullanıcı bulunamadı.");
                return View(model);
            }

            // Şifreyi kontrol et ve giriş yap
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home"); // Başarılıysa Anasayfaya git
            }

            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        // 3. Çıkış Yap
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        
        // 4. Yetkisiz Giriş Hatası Sayfası
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}