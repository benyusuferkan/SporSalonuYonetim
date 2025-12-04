using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Web.Entities;

namespace SporSalonuYonetim.Web.Data
{
    // IdentityDbContext'ten miras alıyoruz ki Üyelik sistemi de gelsin
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Bizim oluşturduğumuz tabloları buraya ekliyoruz
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Fiyat alanı için hassasiyet ayarı (Toplam 18 basamak, virgülden sonra 2 basamak)
builder.Entity<Service>()
    .Property(s => s.Price)
    .HasColumnType("decimal(18,2)");

            // İlişkileri ve ayarları burada belirtebiliriz (Opsiyonel ama sağlam olsun)
            
            // Bir Randevunun sadece bir Üyesi, bir Eğitmeni ve bir Hizmeti olur.
            // Ama bir Eğitmenin çok randevusu olabilir.
            
            builder.Entity<Appointment>()
                .HasOne(a => a.Trainer)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TrainerId)
                .OnDelete(DeleteBehavior.Restrict); // Eğitmen silinirse geçmiş randevular silinmesin

            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany(a => a.Appointments)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}