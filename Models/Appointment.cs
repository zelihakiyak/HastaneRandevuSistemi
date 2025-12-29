namespace HastaneRandevuSistemi.Models
{
    public enum AppointmentStatus
    {
        Active = 0,
        Completed = 1,
        Cancelled = 2,
        Missed = 3
    }
    public class Appointment
    {
        public int Id { get; set; }

        // İlişki: Hangi Doktor?
        public int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; }

        // İlişki: Hangi Hasta? (YENİ EKLENDİ)
        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; }

        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public bool IsActive { get; set; } = true;

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        

    }
    
}
