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

        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Full Name")]
        public string ClientName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string ClientEmail { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string ClientPhone { get; set; }

        // ✅ AJOUTS nécessaires pour créer Client (selon Client.cs)
        [Required(ErrorMessage = "Driver license number is required.")]
        [Display(Name = "Driver License Number")]
        public string DriverLicenseNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        // optionnel: si tu veux séparer plutôt que parser
        // public string FirstName { get; set; }
        // public string LastName { get; set; }

        public decimal DailyRate { get; set; }
        public decimal EstimatedTotal { get; set; }
        public string VehicleName { get; set; }
    }
}
