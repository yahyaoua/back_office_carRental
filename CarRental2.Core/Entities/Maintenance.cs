// Dans CarRental2.Core/Entities/Maintenance.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace CarRental2.Core.Entities
{
    public class Maintenance
    {
        // 🔑 PK: MaintenanceId (GUID)
        public Guid MaintenanceId { get; set; }

        // 🔗 FK: VehicleId (Le véhicule concerné par l'entretien)
        public Guid VehicleId { get; set; }

        // ScheduledDate (Date prévue ou réalisée)
        public DateTime ScheduledDate { get; set; }

        // Type (Oil, Tires, Inspection, etc.)
        [Required]
        [StringLength(100)]
        public string Type { get; set; }

        // Status (Pending, Done, Cancelled, etc.)
        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        // Notes / Coût / Détails de l'intervention
        public string Notes { get; set; }

        /* * Propriété de Navigation (pour EF Core) */

        // Relation N-1 vers Vehicle
        // L'intervention concerne un seul véhicule
        public virtual Vehicle Vehicle { get; set; }
    }
}