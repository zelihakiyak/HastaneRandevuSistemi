namespace HastaneRandevuSistemi.ViewModels
{
    public class DoctorDashboardViewModel
    {
        public string DoctorName { get; set; } = string.Empty;
        public List<DoctorAppointmentDto> TodaysAppointments { get; set; } = new();
        public List<DoctorAppointmentDto> PastAppointments { get; set; } = new();
    }

    public class DoctorAppointmentDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
    }
}