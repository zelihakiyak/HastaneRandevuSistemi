using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class AppointmentCreateViewModel
{
   
    [Required(ErrorMessage = "Lütfen bölüm seçiniz.")]
    public string SelectedDepartmentName { get; set; } = string.Empty;
    public List<string>? Departments { get; set; }

    [Required(ErrorMessage = "Lütfen doktor seçiniz.")]
    public int SelectedDoctorId { get; set; }
    public SelectList? Doctors { get; set; }

    [Required(ErrorMessage = "Lütfen tarih seçiniz.")]
    public DateTime SelectedDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Lütfen saat seçiniz.")]
    public string SelectedSlot { get; set; }
}