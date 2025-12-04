using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için gerekli kütüphane
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

        // DEĞİŞİKLİK 1: Sayfa açılırken Branşları (Hizmetleri) yükle
        public IActionResult Create()
        {
            // Dropdown için verileri hazırla: (Kaydedilecek Değer: Name, Ekranda Görünen: Name)
            ViewBag.Services = new SelectList(_context.Services, "Name", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Trainer trainer, IFormFile photoFile)
        {
            // Validasyon: Fotoğraf yüklenmiş mi?
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
                // Fotoğraf yoksa varsayılan resim ata (Hata vermemesi için)
                trainer.PhotoUrl = "default.png";
            }

            // Kaydetme İşlemi
            if (ModelState.IsValid)
            {
                _context.Trainers.Add(trainer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // DEĞİŞİKLİK 2: Eğer hata varsa Dropdown'ı TEKRAR doldur (Yoksa sayfa patlar!)
            ViewBag.Services = new SelectList(_context.Services, "Name", "Name");
            return View(trainer);
        }

        public IActionResult Delete(int id)
        {
            var trainer = _context.Trainers.Find(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}