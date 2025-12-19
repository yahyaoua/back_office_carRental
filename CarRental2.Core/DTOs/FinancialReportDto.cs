using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarRental2.Core.DTOs
{
    public class FinancialReportDto
    {
        // Période du rapport
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Liste détaillée pour la DataGrid
        public List<FinancialDetailLineDto> Details { get; set; } = new List<FinancialDetailLineDto>();

        // Agrégats (Totaux en bas de page)
        public decimal TotalAmountDue => Details.Sum(x => x.TotalReservationPrice);
        public decimal TotalPaymentsCollected => Details.Sum(x => x.AmountPaid);
        public decimal TotalRemainingBalance => TotalAmountDue - TotalPaymentsCollected;
        public int TotalReservationsCount => Details.Count;
    }

    public class FinancialDetailLineDto
    {
        public Guid ReservationId { get; set; }
        public DateTime ReservationDate { get; set; }
        public string ClientFullName { get; set; }
        public string VehicleModel { get; set; }

        public decimal TotalReservationPrice { get; set; } // Prix théorique total
        public decimal AmountPaid { get; set; }            // Ce qui a été réellement payé
        public decimal Balance => TotalReservationPrice - AmountPaid; // Écart

        public string Status { get; set; } // Ex: "Terminée", "En cours"
    }
}
