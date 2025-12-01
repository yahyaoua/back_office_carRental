// Dans CarRental2.Core/Entities/Payment.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace CarRental2.Core.Entities
{
    public class Payment
    {
        // 🔑 PK: PaymentId (GUID)
        public Guid PaymentId { get; set; }

        // 🔗 FK: ReservationId
        public Guid ReservationId { get; set; }

        // Amount (Montant)
        [Required]
        public decimal Amount { get; set; }

        // PaymentDate
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // PaymentMethod (Card, Cash, Transfer)
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }
        // Note : Utiliser une Enum (PaymentMethod) est fortement recommandé ici.

        // Status (Pending, Completed, Failed)
        [Required]
        [StringLength(50)]
        public string Status { get; set; }
        // Note : Utiliser une Enum (PaymentStatus) est fortement recommandé ici.

        /* * Propriété de Navigation (pour EF Core) */

        // Relation N-1 vers Reservation
        // Le paiement est lié à une seule réservation
        public virtual Reservation Reservation { get; set; }
    }
}