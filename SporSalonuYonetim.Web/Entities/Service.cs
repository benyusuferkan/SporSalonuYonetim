namespace SporSalonuYonetim.Web.Entities
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; } // Hizmet adı (Pilates, Kardiyo)
        public int Duration { get; set; } // Süre (dakika cinsinden)
        public decimal Price { get; set; } // Ücret
        public string Description { get; set; }
        
        // Bu hizmete ait randevular
        public List<Appointment> Appointments { get; set; }
    }
}