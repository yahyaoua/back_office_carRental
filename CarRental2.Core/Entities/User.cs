// Dans CarRental2.Core/Entities/User.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Utilisé ici pour la clarté

namespace CarRental2.Core.Entities
{
    public class User
    {
        // 🔑 PK: UserId (GUID)
        public Guid UserId { get; set; }

        // Username (pour la connexion)
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        // Email
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        // PasswordHash (stockage sécurisé)
        [Required]
        public string PasswordHash { get; set; }

        // FullName
        [Required]
        [StringLength(255)]
        public string FullName { get; set; }

        // Role (Admin, Manager, Agent)
        [Required]
        [StringLength(50)]
        public string Role { get; set; }
        

        // IsActive (bit)
        public bool IsActive { get; set; }

        // CreatedAt, UpdatedAt (Horodatage d'audit)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; }

        /* * Propriétés de Navigation (pour EF Core)
         * Un User peut avoir créé plusieurs réservations s'il s'agit d'un agent de location (FK CreatedByUserId dans Reservation)
        */
        public virtual ICollection<Reservation> CreatedReservations { get; set; } = new List<Reservation>();

        // Si vous implémentez l'AuditLog, l'utilisateur sera lié à plusieurs actions dans le journal.
        // public virtual ICollection<AuditLog> AuditLogs { get; set; }
    }
}