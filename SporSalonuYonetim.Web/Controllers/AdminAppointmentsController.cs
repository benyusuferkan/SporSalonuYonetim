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

        // 2. GENEL ONAYLA (Randevu ilk alındığında onaylamak için)
        public IActionResult Approve(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = "Onaylandı";
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Randevu durumu 'Onaylandı' olarak güncellendi.";
            }
            return RedirectToAction("Index");
        }

        // 3. GENEL REDDET (Randevu ilk alındığında reddetmek için)
        public IActionResult Reject(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = "Reddedildi";
                _context.SaveChanges();
                TempData["WarningMessage"] = "Randevu reddedildi.";
            }
            return RedirectToAction("Index");
        }

        // 4. MANUEL SİL (Admin kafasına göre silmek isterse)
        public IActionResult Delete(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Randevu başarıyla silindi.";
            }
            return RedirectToAction("Index");
        }

        // --- YENİ EKLENEN: İPTAL İSTEKLERİNİ YÖNETME ---

        // 5. İPTAL İSTEĞİNİ ONAYLA (Üyenin isteğini kabul et ve sil)
        public IActionResult ApproveCancellation(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment != null)
            {
                // İstek onaylandığı için randevuyu siliyoruz (Firmayı zarara uğratmamak için yer açılıyor)
                _context.Appointments.Remove(appointment);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "İptal talebi onaylandı ve randevu silindi.";
            }
            return RedirectToAction("Index");
        }

        // 6. İPTAL İSTEĞİNİ REDDET (Üyenin isteğini geri çevir, randevu kalsın)
        public IActionResult RejectCancellation(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment != null)
            {
                // Silme yapmıyoruz, sadece iptal bayrağı kaldırıyoruz
                appointment.IsCancellationRequested = false; 
                _context.SaveChanges();
                TempData["WarningMessage"] = "İptal talebi reddedildi, randevu hala geçerli.";
            }
            return RedirectToAction("Index");
        }
    }
}