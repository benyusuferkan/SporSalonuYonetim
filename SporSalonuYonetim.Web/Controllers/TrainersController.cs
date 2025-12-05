using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SporSalonuYonetim.Web.Data;
using SporSalonuYonetim.Web.Entities;

namespace SporSalonuYonetim.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public TrainersController(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View(_context.Trainers.ToList());
        }

        public IActionResult Create()
        {
            ViewBag.Services = new SelectList(_context.Services, "Name", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Trainer trainer, IFormFile photoFile)
        {
            if (photoFile != null)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + photoFile.FileName;
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(fileStream);
                }
                trainer.PhotoUrl = uniqueFileName;
            }
            else
            {
                trainer.PhotoUrl = "default.png";
            }

            if (ModelState.IsValid)
            {
                _context.Trainers.Add(trainer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Services = new SelectList(_context.Services, "Name", "Name");
            return View(trainer);
        }

        // --- DÜZELTİLEN KISIM BURASI ---
        public IActionResult Delete(int id)
        {
            // 1. Önce Kontrol Et: Bu hocanın randevusu var mı?
            // (Veritabanında Appointments tablosunda bu TrainerId ile eşleşen kayıt var mı?)
            bool hasAppointments = _context.Appointments.Any(x => x.TrainerId == id);

            if (hasAppointments)
            {
                // Eğer randevusu varsa silme! Hata mesajı yükle ve geri gönder.
                TempData["ErrorMessage"] = "Bu antrenörün aktif veya geçmiş randevuları olduğu için SİLİNEMEZ! Lütfen önce randevuları iptal edin.";
                return RedirectToAction("Index");
            }

            // 2. Eğer randevusu yoksa silme işlemine devam et
            var trainer = _context.Trainers.Find(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Antrenör başarıyla silindi.";
            }

            return RedirectToAction("Index");
        }
    }
}