using System;

namespace CarRental.Api.ViewModels
{
    public class MyReservationItemViewModel
    {
        public Guid ReservationId { get; set; }

        // Véhicule
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleBrand { get; set; } = string.Empty;

        // Dates
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Prix
        public decimal DailyRate { get; set; }
        public decimal TotalPrice { get; set; }

        public decimal TotalAmount { get; set; }

        // Statut
        public string Status { get; set; } = string.Empty;
    }
}
