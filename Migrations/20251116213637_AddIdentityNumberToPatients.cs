using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HastaneRandevuSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityNumberToPatients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ad",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "Telefon",
                table: "Patients",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "Soyad",
                table: "Patients",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Patients",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "HastaID",
                table: "Patients",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "Specialty",
                table: "Doctors",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Doctors",
                newName: "Department");

            migrationBuilder.RenameColumn(
                name: "DoctorID",
                table: "Doctors",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IsCompleted",
                table: "Appointments",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Appointments",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "Patients",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AppointmentDate",
                table: "Appointments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "AppointmentTime",
                table: "Appointments",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AppointmentDate",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AppointmentTime",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Patients",
                newName: "Telefon");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Patients",
                newName: "Soyad");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Patients",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Patients",
                newName: "HastaID");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Doctors",
                newName: "Specialty");

            migrationBuilder.RenameColumn(
                name: "Department",
                table: "Doctors",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Doctors",
                newName: "DoctorID");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Appointments",
                newName: "IsCompleted");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Appointments",
                newName: "Date");

            migrationBuilder.AddColumn<string>(
                name: "Ad",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
