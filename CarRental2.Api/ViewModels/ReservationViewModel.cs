using System;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Api.ViewModels
{
    public class ReservationViewModel
    {
        [Required(ErrorMessage = "Vehicle ID is missing.")]
        public Guid VehicleId { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Requested Pickup Date")]
        public DateTime RequestedStart { get; set; }

        [Required(ErrorMessage = "End Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Requested Return Date")]
        public DateTime RequestedEnd { get; set; }

        // ================= CLIENT =================

        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        // (optionnel – si encore utilisé ailleurs)
        public string ClientName => $"{FirstName} {LastName}".Trim();

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string ClientEmail { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? ClientPhone { get; set; }

        [Required(ErrorMessage = "Driver license number is required.")]
        [Display(Name = "Driver License Number")]
        public string DriverLicenseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birth date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        // ================= AFFICHAGE =================

        public decimal DailyRate { get; set; }
        public decimal EstimatedTotal { get; set; }

        // ⚠️ affichage seulement (PAS Required)
        public string? VehicleName { get; set; }
    }
}
