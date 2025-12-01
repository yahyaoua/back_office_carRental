// Dans CarRental2.Core/Entities/VehicleImage.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace CarRental2.Core.Entities
{
    public class VehicleImage
    {
        // 🔑 PK: VehicleImageId (GUID) - Utiliser Guid pour rester cohérent
        public Guid VehicleImageId { get; set; }

        // 🔗 FK: VehicleId
        public Guid VehicleId { get; set; }

        // ImagePath / Blob / URL
        [Required]
        public string ImagePath { get; set; } // Chemin d'accès ou URL vers le fichier image

        // IsPrimary (booléen)
        public bool IsPrimary { get; set; } // Indique si c'est l'image principale pour l'aperçu

        /* * Propriété de Navigation (pour EF Core) */

        // Relation N-1 vers Vehicle
        // L'image appartient à un seul véhicule
        public virtual Vehicle Vehicle { get; set; }
    }
}
