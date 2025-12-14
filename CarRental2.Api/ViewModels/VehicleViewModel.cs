namespace CarRental.Api.ViewModels
{
    /// <summary>
    /// ViewModel used for listing and displaying brief details of a rental vehicle
    /// on the client-facing web interface.
    /// </summary>
    public class VehicleViewModel
    {
        // 🔑 Primary Key needed for linking to the reservation form
        public Guid VehicleId { get; set; }

        // --- Core Identification ---
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }

        // --- Pricing ---
        // Maps directly from the entity's BaseRatePerDay.
        public decimal DailyRate { get; set; }

        // --- Vehicle Characteristics (Derived from Navigation Properties) ---

        // This holds the friendly name (e.g., "Sedan", "SUV") from the VehicleType entity.
        public string VehicleType { get; set; }

        // --- Status and Presentation ---

        // The image URL for display (we assume the first image from the Images collection is used).
        public string ImageUrl { get; set; }

        // Optional: Include status only if you want the client to see "Reserved" or "Available".
        public string Status { get; set; }
    }
}