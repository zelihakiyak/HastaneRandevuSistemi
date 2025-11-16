using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.ViewModels;

namespace HastaneRandevuSistemi.Services
{
    public interface IAuthService
    {
        Task<(bool IsSuccess, string Message)> RegisterPatientAsync(PatientRegisterViewModel model);
        Task<(bool IsSuccess, string Message)> RegisterDoctorAsync(DoctorRegisterViewModel model);

        Task<Patient> ValidatePatientCredentialsAsync(LoginViewModel model);
        Task<Doctor> ValidateDoctorCredentialsAsync(LoginViewModel model);

    }
}