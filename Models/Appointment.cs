namespace HastaneRandevuSistemi.Models
{
    public enum AppointmentStatus
    {
        Active = 1,
        Completed = 2,
        Cancelled = 3,
        Missed = 4
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
/// <summary>
/// 
/// </summary>
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        

    }
    
}
