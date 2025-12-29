using HastaneRandevuSistemi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace HastaneRandevuSistemi.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
		public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Department> Departments { get; set; }
    }
}
