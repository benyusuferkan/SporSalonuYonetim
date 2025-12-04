using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Web.Data;
using SporSalonuYonetim.Web.Entities;

namespace SporSalonuYonetim.Web.Controllers
{
    [Authorize] // Sadece giriş yapmış üyeler görebilir
    public class AppointmentController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        // 1. RANDEVULARIM SAYFASI
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointments = _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.MemberId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return View(appointments);
        }

        // 2. RANDEVU ALMA EKRANI (GET)
        public IActionResult Create()
        {
            // Hizmetleri dolduruyoruz
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name");
            
            // Eğitmenleri başta BOŞ gönderiyoruz (veya tümünü), çünkü hizmet seçilince JavaScript ile dolacak
            ViewBag.Trainers = new SelectList(_context.Trainers, "Id", "FullName");
            
            return View();
        }

        // 3. YENİ EKLENEN: HİZMETE GÖRE HOCALARI GETİREN METOT (AJAX İÇİN)
        [HttpGet]
        public IActionResult GetTrainersByService(int serviceId)
        {
            // 1. Seçilen hizmetin adını bul (Örn: "Yoga")
            var service = _context.Services.Find(serviceId);
            if (service == null) return Json(new List<object>());

            // 2. Uzmanlığı bu hizmetin adı olan hocaları bul
            var trainers = _context.Trainers
                .Where(t => t.Specialty == service.Name) // Uzmanlık alanı eşleşenler
                .Select(t => new {
                    id = t.Id,
                    fullName = t.FullName
                })
                .ToList();

            return Json(trainers);
        }

        // 4. RANDEVU KAYDETME İŞLEMİ (POST)
        [HttpPost]
        public IActionResult Create(Appointment appointment)
        {
            // Dropdownlar boş gelirse diye tekrar doldur
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name");
            ViewBag.Trainers = new SelectList(_context.Trainers, "Id", "FullName");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            appointment.MemberId = userId;
            appointment.CreatedDate = DateTime.Now;
            appointment.Status = "Onay Bekliyor"; // Admin onayı gerektiği için "Beklemede" yapalım

            var selectedService = _context.Services.Find(appointment.ServiceId);
            if (selectedService == null) return View(appointment);

            DateTime newStart = appointment.AppointmentDate;
            DateTime newEnd = newStart.AddMinutes(selectedService.Duration);

            // --- ÇAKIŞMA KONTROLÜ ---
            bool isConflict = _context.Appointments.Any(x =>
                x.TrainerId == appointment.TrainerId &&
                x.Status != "Reddedildi" && // Reddedilen randevular çakışma yaratmaz
                x.AppointmentDate < newEnd &&
                x.AppointmentDate.AddMinutes(x.Service.Duration) > newStart
            );

            if (isConflict)
            {
                ModelState.AddModelError("", "⚠️ Seçtiğiniz saatte bu antrenörün başka bir randevusu var. Lütfen başka bir saat seçin.");
                return View(appointment);
            }

            if (ModelState.IsValid)
            {
                _context.Appointments.Add(appointment);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(appointment);
        }
    }
}