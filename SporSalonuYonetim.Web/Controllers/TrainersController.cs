using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SporSalonuYonetim.Web.Data;
using SporSalonuYonetim.Web.Entities;

namespace SporSalonuYonetim.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // Dosya yolu bulucu

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
            return View();
        }

        // FOTOĞRAF YÜKLEME İŞLEMİ BURADA
        [HttpPost]
        public async Task<IActionResult> Create(Trainer trainer, IFormFile photoFile)
        {
            // Validasyon kontrolü (Fotoğraf seçildi mi?)
            if (photoFile != null)
            {
                // 1. Dosya adını benzersiz yap (örn: guid-resim.jpg)
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + photoFile.FileName;
                
                // 2. Kaydedilecek klasör yolu (wwwroot/images)
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 3. Dosyayı kaydet
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(fileStream);
                }

                // 4. Veritabanına dosya adını yaz
                trainer.PhotoUrl = uniqueFileName;
            }
            else
            {
                // Fotoğraf yüklemezse varsayılan bir resim atayabiliriz
                trainer.PhotoUrl = "default.png"; 
            }

            // ModelState validasyonu (Hata yoksa kaydet)
            // Not: PhotoUrl required ise, yukarıda atadığımız için valid geçer.
            _context.Trainers.Add(trainer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
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