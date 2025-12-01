// Dans CarRental2.Core/Entities/Client.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;// Utilisé pour les annotations (facultatif mais recommandé pour EF Core)

namespace CarRental2.Core.Entities
{
    public class Client
    {
        // 🔑 PK: ClientId (GUID)
        public Guid ClientId { get; set; }

        // FirstName, LastName
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        // Email, Phone
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        // Address
        public string Address { get; set; } // Laisser sans StringLength si vous prévoyez de longues adresses

        // DriverLicenseNumber
        [Required]
        [StringLength(50)]
        public string DriverLicenseNumber { get; set; }

        // BirthDate
        public DateTime BirthDate { get; set; }

        // CreatedAt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /* * Propriété de Navigation (pour EF Core)
         * Un client peut avoir plusieurs réservations (Relation 1 à N)
        */
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}