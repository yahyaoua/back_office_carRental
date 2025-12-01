// Dans CarRental2.Core/Entities/Reservation.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CarRental2.Core.Entities
{
    public class Reservation
    {
        // 🔑 PK: ReservationId (GUID)
        public Guid ReservationId { get; set; }

        // 🔗 FK: ClientId (Le client qui loue)
        public Guid ClientId { get; set; }

        // 🔗 FK: VehicleId (Le véhicule attribué. Peut être null au début)
        public Guid? VehicleId { get; set; } // Utilisation de Guid? pour autoriser la valeur NULL

        // Dates demandées (par le client ou l'agent)
        public DateTime RequestedStart { get; set; }
        public DateTime RequestedEnd { get; set; }

        // Dates réelles (Départ/Retour du véhicule)
        public DateTime? ActualStart { get; set; } // Nullable, rempli au moment du départ
        public DateTime? ActualEnd { get; set; }   // Nullable, rempli au moment du retour

        // Status (Pending, Confirmed, Active, Completed, Cancelled, NoShow)
        [Required]
        [StringLength(50)]
        public string Status { get; set; }
        // Note : Utiliser des constantes ou une Enum (ReservationStatus) est préférable ici.

        // Montants
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; }

        // 🔗 FK: CreatedByUserId (L'employé qui a créé ou validé la réservation)
        // Nullable si la réservation est faite directement par le client (Front-office)
        public Guid? CreatedByUserId { get; set; }

        // Documents et QR Code
        public string QRCodeData { get; set; }
        public string InvoicePdfPath { get; set; } // Chemin vers le PDF du bon de réservation/facture

        /* * Propriétés de Navigation (pour EF Core) */

        // 1. Relation N-1 vers Client
        public virtual Client Client { get; set; }

        // 2. Relation N-1 vers Vehicle (Peut être null)
        public virtual Vehicle Vehicle { get; set; }

        // 3. Relation N-1 vers User (L'employé qui a géré la transaction)
        public virtual User CreatedByUser { get; set; }

        // 4. Relation 1-N vers Payment
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
