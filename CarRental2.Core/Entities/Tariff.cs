// Dans CarRental2.Core/Entities/Tariff.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace CarRental2.Core.Entities
{
    public class Tariff
    {
        // 🔑 PK: TariffId (GUID)
        public Guid TariffId { get; set; }

        // 🔗 FK: VehicleTypeId (Permet d'appliquer le tarif à tous les véhicules de ce type)
        public Guid VehicleTypeId { get; set; }

        // StartDate, EndDate (Période de validité du tarif)
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        // PricePerDay, PricePerHour (Tarifs)
        [Required]
        public decimal PricePerDay { get; set; }

        public decimal PricePerHour { get; set; }

        // Description
        [StringLength(255)]
        public string Description { get; set; }

        /* * Propriété de Navigation (pour EF Core) */

        // Relation N-1 vers VehicleType
        // Le tarif est défini pour un seul type de véhicule
        public virtual VehicleType VehicleType { get; set; }
    }
}
