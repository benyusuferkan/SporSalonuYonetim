using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Web.Data;

namespace SporSalonuYonetim.Web.Controllers
{
    [Route("api/[controller]")] // Adresimiz: /api/reports olacak
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TÜM ANTRENÖRLERİ GETİR (JSON RAPORU)
        // Adres: GET /api/reports/trainers
        [HttpGet("trainers")]
        public async Task<IActionResult> GetTrainers()
        {
            var trainers = await _context.Trainers
                .Select(t => new { 
                    t.Id,
                    t.FullName,
                    Uzmanlik = t.Specialty,
                    ToplamRandevuSayisi = t.Appointments.Count() // LINQ ile sayım yapıyoruz
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // 2. DETAYLI RANDEVU RAPORU (FİLTRELEMELİ)
        // Adres: GET /api/reports/appointments?trainerId=1
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments(int? trainerId)
        {
            // Veritabanından randevuları çekmeye başla
            var query = _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .AsQueryable();

            // EĞER URL'de hoca ID'si varsa filtrele (LINQ Where)
            if (trainerId.HasValue)
            {
                query = query.Where(a => a.TrainerId == trainerId.Value);
            }

            // Sonuçları hazırla (İstenen formatta)
            var results = await query
                .Select(a => new {
                    RandevuId = a.Id,
                    Hoca = a.Trainer.FullName,
                    Ders = a.Service.Name,
                    Tarih = a.AppointmentDate.ToString("yyyy-MM-dd HH:mm"),
                    Durum = a.Status
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}
