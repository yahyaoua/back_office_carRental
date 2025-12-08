// Dans CarRental2.Core/Entities/Maintenance.cs

using System;
using System.ComponentModel.DataAnnotations;
// Si votre entité hérite de BaseEntity (comme dans mon exemple précédent)
// using CarRental2.Core.Entities.Base; 

namespace CarRental2.Core.Entities
{
    public class Maintenance
    {
        // 🔑 PK: MaintenanceId (GUID)
        public Guid MaintenanceId { get; set; }

        // NOUVELLE PROPRIÉTÉ CALCULÉE (CORRECTION DU PROBLÈME ID PARTIEL)
        /// <summary>
        /// Retourne l'ID de maintenance tronqué pour l'affichage (ex: '26a14e07...').
        /// </summary>
        public string MaintenanceIdShort => MaintenanceId.ToString().Length >= 8
            ? MaintenanceId.ToString().Substring(0, 8) + "..."
            : MaintenanceId.ToString();

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