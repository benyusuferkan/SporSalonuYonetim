namespace SporSalonuYonetim.Web.Entities
{
    public class Trainer
    {
        public int Id { get; set; }
        public string FullName { get; set; } // Ad Soyad
        public string Specialty { get; set; } // Uzmanlık (Fitness, Yoga vb.)
        public string Bio { get; set; } // Kısa özgeçmiş
        public string PhotoUrl { get; set; } // Fotoğraf linki

        // Eğitmenin randevuları
        public List<Appointment> Appointments { get; set; }
    }
}