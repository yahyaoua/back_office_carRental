using CarRental.Api.Data;
using CarRental2.Core.DTOs;
using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Api.Services
{
    public class FinancialReportService : IFinancialReportService
    {
        private readonly ApplicationDbContext _context;

        public FinancialReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FinancialReportDto> GetFinancialReportAsync(DateTime start, DateTime end)
        {
            // Note: On filtre sur RequestedStart car ActualStart est null pour les réservations futures
            var reservations = await _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.VehicleType) // Charger VehicleType pour construire le libellé
                .Include(r => r.Payments)
                .Where(r => r.RequestedStart >= start && r.RequestedStart <= end)
                .Select(r => new FinancialDetailLineDto
                {
                    // Utilisation de ReservationId (Guid)
                    ReservationId = r.ReservationId,
                    ReservationDate = r.RequestedStart,
                    ClientFullName = r.Client.FirstName + " " + r.Client.LastName,
                    // Gestion du cas où le véhicule n'est pas encore attribué (null)
                    VehicleModel = r.Vehicle != null
                        ? ((r.Vehicle.VehicleType != null ? r.Vehicle.VehicleType.Name + " " : "") + r.Vehicle.Model)
                        : "Non attribué",
                    // Mapping des montants financiers
                    TotalReservationPrice = r.TotalAmount,
                    AmountPaid = r.Payments != null ? r.Payments.Sum(p => p.Amount) : 0m,
                    Status = r.Status
                })
                .ToListAsync();

            return new FinancialReportDto
            {
                StartDate = start,
                EndDate = end,
                Details = reservations
            };
        }
    }
}