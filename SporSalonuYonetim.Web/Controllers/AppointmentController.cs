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

        // 1. RANDEVULARIM SAYFASI (Geçmiş ve Gelecek)
        public IActionResult Index()
        {
            // Giriş yapan kullanıcının ID'sini bul
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Sadece bu kullanıcıya ait randevuları getir, hocası ve dersiyle beraber
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
            // Dropdown (Açılır Kutu) için verileri hazırla
            ViewBag.Trainers = new SelectList(_context.Trainers, "Id", "FullName");
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name");
            return View();
        }

        // 3. RANDEVU KAYDETME İŞLEMİ (POST) - KRİTİK YER!
        [HttpPost]
        public IActionResult Create(Appointment appointment)
        {
            // Dropdownlar boş gelirse diye tekrar dolduruyoruz (Hata durumunda lazım)
            ViewBag.Trainers = new SelectList(_context.Trainers, "Id", "FullName");
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name");

            // 1. Kullanıcıyı ata
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            appointment.MemberId = userId;
            appointment.CreatedDate = DateTime.Now;
            appointment.Status = "Onaylandı"; // Şimdilik direkt onaylayalım

            // 2. Seçilen Hizmetin Süresini Bul (Bitiş saatini hesaplamak için)
            var selectedService = _context.Services.Find(appointment.ServiceId);
            if (selectedService == null) return View(appointment);

            // Yeni Randevunun Başlangıç ve Bitiş Saati
            DateTime newStart = appointment.AppointmentDate;
            DateTime newEnd = newStart.AddMinutes(selectedService.Duration);

            // --- ÇAKIŞMA KONTROLÜ (CONFLICT CHECK) --- 
            // Veritabanına sor: "Bu hocanın, bu saat aralığında başka işi var mı?"
            bool isConflict = _context.Appointments.Any(x =>
                x.TrainerId == appointment.TrainerId && // Aynı hoca
                x.AppointmentDate < newEnd &&           // Mevcut işin başı, yeni işin bitişinden önceyse
                x.AppointmentDate.AddMinutes(x.Service.Duration) > newStart // Mevcut işin sonu, yeni işin başından sonraysa
            );

            if (isConflict)
            {
                // Hata Mesajı Fırlat
                ModelState.AddModelError("", "⚠️ Seçtiğiniz saatte bu antrenörün başka bir randevusu var. Lütfen başka bir saat seçin.");
                return View(appointment);
            }

            // Sorun yoksa kaydet
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