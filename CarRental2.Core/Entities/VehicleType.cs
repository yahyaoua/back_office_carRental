// Dans CarRental2.Core/Entities/VehicleType.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CarRental2.Core.Entities
{
    public class VehicleType
    {
        // 🔑 PK: VehicleTypeId (GUID)
        public Guid VehicleTypeId { get; set; }

        // Name (Nom de la catégorie)
        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Ex: Berline, SUV, Utilitaire

        // Description
        public string Description { get; set; }

        /* * Propriétés de Navigation (pour EF Core) */

        // 1. Relation 1-N vers Vehicle
        // Un type possède plusieurs véhicules
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        // 2. Relation 1-N vers Tariff
        // Un type peut avoir plusieurs règles de tarification (saison, période)
        public virtual ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
    }
}