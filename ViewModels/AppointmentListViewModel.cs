using System.Collections.Generic;

namespace HastaneRandevuSistemi.ViewModels
{
    public class AppointmentListViewModel
    {
        public List<AppointmentDto> ActiveAppointments { get; set; } = new List<AppointmentDto>();
        public List<AppointmentDto> PastAppointments { get; set; } = new List<AppointmentDto>();
    }

    public class AppointmentDto
    {
        public int Id { get; set; }
        public string DateFormatted { get; set; }
        public string TimeFormatted { get; set; }
        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }
        public string StatusClass { get; set; }
        public string StatusLabel { get; set; }
        public bool CanCancel { get; set; }
    }
}