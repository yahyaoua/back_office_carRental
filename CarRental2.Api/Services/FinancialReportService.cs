using CarRental.Api.Data;
using CarRental2.Core.DTOs;
using CarRental2.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using CarRental.Api.Services;

namespace CarRental.Api.Services
{
    public class FinancialReportService : IFinancialReportService
    {
        private readonly ApplicationDbContext _context;

        public FinancialReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FinancialSummaryDto> GetMonthlySummaryAsync(DateTime startDate, DateTime endDate)
        {
            //  Calculer le Montant Total des Réservations
           
            decimal totalReservationRevenue = await _context.Reservations
                .Where(r => r.RequestedStart >= startDate && r.RequestedStart <= endDate)
                .SumAsync(r => r.TotalAmount); // Appelle SumAsync directement sur la propriété

            
            decimal totalPaymentsReceived = await _context.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .SumAsync(p => p.Amount); 

            // Compter le Nombre Total de Réservations 
            int totalReservationsCount = await _context.Reservations
                .CountAsync(r => r.RequestedStart >= startDate && r.RequestedStart <= endDate);
            return new FinancialSummaryDto
            {
                TotalReservationRevenue = totalReservationRevenue,
                TotalPaymentsReceived = totalPaymentsReceived,
                TotalReservationsCount = totalReservationsCount,
                StartDate = startDate,
                EndDate = endDate
            };
        }
    }
}