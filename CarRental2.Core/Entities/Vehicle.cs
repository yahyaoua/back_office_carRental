// Dans CarRental2.Core/Entities/Vehicle.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CarRental2.Core.Entities
{
    public class Vehicle
    {
        // 🔑 PK: VehicleId (GUID)
        public Guid VehicleId { get; set; }

        // PlateNumber (Immatriculation)
        [Required]
        [StringLength(20)]
        public string PlateNumber { get; set; }

        // Make, Model, Year
        [Required]
        [StringLength(100)]
        public string Make { get; set; }

        [Required]
        [StringLength(100)]
        public string Model { get; set; }

        public int Year { get; set; }

        // 🔗 FK: VehicleTypeId
        public Guid VehicleTypeId { get; set; }

        // Mileage (int)
        public int Mileage { get; set; } // Kilométrage actuel

        // Status (Available, Rented, Maintenance, Reserved)
        [Required]
        [StringLength(50)]
        public string Status { get; set; }
        // Note: Idéalement, utilisez ici une enum comme VehicleStatus.

        // BaseRatePerDay (decimal)
        public decimal BaseRatePerDay { get; set; }

        // NextMaintenanceDate (date)
        public DateTime NextMaintenanceDate { get; set; }

        // CreatedAt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /* * Propriétés de Navigation (pour EF Core) */

        // 1. Relation N-1 vers VehicleType
        public virtual VehicleType VehicleType { get; set; }

        // 2. Relation 1-N vers VehicleImage
        public virtual ICollection<VehicleImage> Images { get; set; } = new List<VehicleImage>();

        // 3. Relation 1-N vers Maintenance
        public virtual ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();

        // 4. Relation 1-N vers Reservation
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}