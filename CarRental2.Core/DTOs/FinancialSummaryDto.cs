using System;
using System.Collections.Generic;
using System.Text;

namespace CarRental2.Core.DTOs
{
     public class FinancialSummaryDto
    {
        public decimal TotalReservationRevenue { get; set; } // Montant total des réservations
        public decimal TotalPaymentsReceived { get; set; }    // Total des paiements enregistrés
        public int TotalReservationsCount { get; set; }      // Nombre total de réservations
        public decimal NetRevenue => TotalPaymentsReceived;  // Revenu Net simple (Payments - Dépenses futures)
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
