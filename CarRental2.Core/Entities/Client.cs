// Dans CarRental2.Core/Entities/Client.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarRental2.Core.Entities
{
    public class Client
    {
        // 🔑 PK
        public Guid ClientId { get; set; }

        // =========================
        // Identity
        // =========================

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        // 🔐 PASSWORD (HASHÉ)
        
        public string PasswordHash { get; set; }

        // =========================
        // Contact
        // =========================

        [StringLength(50)]
        public string Phone { get; set; }

        public string Address { get; set; }

        // =========================
        // Legal info
        // =========================

        [Required]
        [StringLength(50)]
        public string DriverLicenseNumber { get; set; }

        public DateTime BirthDate { get; set; }

        // =========================
        // Metadata
        // =========================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // =========================
        // Navigation
        // =========================

        public virtual ICollection<Reservation> Reservations { get; set; }
            = new List<Reservation>();
    }
}
