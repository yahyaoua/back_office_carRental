using System.Collections.Generic;

namespace CarRental.Api.ViewModels
{
    public class MyReservationsViewModel
    {
        public string ClientName { get; set; } = string.Empty;

        public List<MyReservationItemViewModel> Reservations { get; set; } = new();

        public int TotalReservations => Reservations.Count;
    }
}
