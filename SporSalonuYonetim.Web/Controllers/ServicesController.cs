using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SporSalonuYonetim.Web.Data;
using SporSalonuYonetim.Web.Entities;

namespace SporSalonuYonetim.Web.Controllers
{
    // Bu sayfaya sadece Admin girebilsin!
    [Authorize(Roles = "Admin")]
    public class ServicesController : Controller
    {
        private readonly AppDbContext _context;

        public ServicesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME SAYFASI
        public IActionResult Index()
        {
            var services = _context.Services.ToList();
            return View(services);
        }

        // 2. EKLEME SAYFASI (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. EKLEME İŞLEMİ (POST)
        [HttpPost]
        public IActionResult Create(Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Yeni hizmet başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            return View(service);
        }
        
        // 4. SİLME İŞLEMİ (GÜVENLİ HALE GETİRİLDİ)
        public IActionResult Delete(int id)
        {
            // EMNİYET KİLİDİ: Bu hizmete ait randevu var mı?
            bool hasAppointments = _context.Appointments.Any(x => x.ServiceId == id);

            if (hasAppointments)
            {
                // Eğer randevu varsa silme! Hata mesajı yükle ve geri gönder.
                TempData["ErrorMessage"] = "Bu hizmete ait planlanmış randevular olduğu için SİLİNEMEZ! Lütfen önce ilgili randevuları iptal edin.";
                return RedirectToAction("Index");
            }

            // Randevu yoksa sil
            var service = _context.Services.Find(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Hizmet başarıyla silindi.";
            }
            return RedirectToAction("Index");
        }
    }
}