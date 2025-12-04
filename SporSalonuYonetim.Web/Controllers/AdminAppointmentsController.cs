using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Web.Data;

namespace SporSalonuYonetim.Web.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Admin girebilir!
    public class AdminAppointmentsController : Controller
    {
        private readonly AppDbContext _context;

        public AdminAppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TÜM RANDEVULARI LİSTELE
        public IActionResult Index()
        {
            var appointments = _context.Appointments
                .Include(a => a.Trainer) // Hoca ismini görmek için
                .Include(a => a.Service) // Ders ismini görmek için
                .OrderByDescending(a => a.AppointmentDate) // En yeni en üstte
                .ToList();
            
            return View(appointments);
        }

        // 2. ONAYLA
        public IActionResult Approve(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = "Onaylandı";
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // 3. REDDET
        public IActionResult Reject(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = "Reddedildi";
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // 4. SİL (Veritabanından tamamen uçurur)
        public IActionResult Delete(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}