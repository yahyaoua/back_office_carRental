namespace CarRental.Api.ViewModels
{
    public class CarViewModel
    {
        // Properties used to display details of a single car

        public int Id { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }

        public int Year { get; set; }

        public string LicensePlate { get; set; }

        public decimal DailyRate { get; set; }

        // Optional: Could be used for display status (e.g., "Available" or "Booked")
        public bool IsAvailable { get; set; }
    }
}