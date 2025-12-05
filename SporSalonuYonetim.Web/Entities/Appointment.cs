namespace SporSalonuYonetim.Web.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        
        // İlişkiler
        public string MemberId { get; set; } // Üye (Identity User ID)
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        public DateTime AppointmentDate { get; set; } // Tarih ve Saat
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Entities/Appointment.cs içine eklenecek:
        public bool IsCancellationRequested { get; set; } = false; // Varsayılan: Hayır
        
        // Durum: Onay Bekliyor, Onaylandı, İptal
        public string Status { get; set; } = "Pending"; 
    }
}