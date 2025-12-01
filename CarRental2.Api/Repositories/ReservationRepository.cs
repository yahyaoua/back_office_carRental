// Dans CarRental.Api/Repositories/ReservationRepository.cs

using CarRental.Api.Data;
using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Api.Repositories
{
    public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
    {
        public ReservationRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        // ========================================================
        // 1. Méthodes de lecture avec détails
        // ========================================================

        public async Task<Reservation> GetReservationWithDetailsAsync(Guid reservationId)
        {
            return await _dbSet
                .Where(r => r.ReservationId == reservationId)
                .Include(r => r.Client)
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.VehicleType)
                .Include(r => r.Payments)
                .Include(r => r.CreatedByUser)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<Reservation>> GetActiveReservationsAsync()
        {
            // Récupère les réservations qui sont 'Pending', 'Confirmed' ou 'Active'
            return await _dbSet
                .Where(r => r.Status == "Pending" || r.Status == "Confirmed" || r.Status == "Active")
                .Include(r => r.Client)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.RequestedStart)
                .ToListAsync();
        }

        // ========================================================
        // 2. Logique de Disponibilité
        // ========================================================

        public async Task<bool> IsVehicleAvailableAsync(Guid vehicleId, DateTime startDate, DateTime endDate)
        {
            // Compte le nombre de réservations actives ou futures qui chevauchent la période
            int conflictingReservationsCount = await _dbSet
                .Where(r => r.VehicleId == vehicleId)
                .Where(r => r.Status != "Completed" && r.Status != "Cancelled" && r.Status != "NoShow")
                .Where(r => r.RequestedStart < endDate && r.RequestedEnd > startDate) // Logique de chevauchement
                .CountAsync();

            // Vérifie également la maintenance dans cette période (bien que souvent géré par un service)
            int conflictingMaintenanceCount = await _dbContext.Maintenances
                 .Where(m => m.VehicleId == vehicleId)
                 .Where(m => m.Status != "Done" && m.Status != "Cancelled")
                 .Where(m => m.ScheduledDate >= startDate && m.ScheduledDate <= endDate)
                 .CountAsync();

            return conflictingReservationsCount == 0 && conflictingMaintenanceCount == 0;
        }

        // NOTE: GetAvailableVehiclesByTypeAsync est TRES similaire à la logique déjà codée
        // dans VehicleRepository. Dans un design optimal, cette méthode pourrait appeler le VehicleRepository.
        // Pour des raisons d'indépendance de la couche, nous implémentons ici la logique :
        public async Task<IReadOnlyList<Vehicle>> GetAvailableVehiclesByTypeAsync(
            Guid vehicleTypeId,
            DateTime startDate,
            DateTime endDate)
        {
            // Étape 1: Identifier les IDs de véhicules indisponibles (réservations)
            var reservedVehicleIds = await _dbContext.Reservations
                .Where(r => r.Status != "Completed" && r.Status != "Cancelled" && r.Status != "NoShow")
                .Where(r => r.VehicleId.HasValue && (r.RequestedStart < endDate && r.RequestedEnd > startDate))
                .Select(r => r.VehicleId.Value)
                .Distinct()
                .ToListAsync();

            // Étape 2: Identifier les IDs de véhicules indisponibles (maintenance)
            var maintenanceVehicleIds = await _dbContext.Maintenances
               .Where(m => m.Status != "Done" && m.Status != "Cancelled")
               .Where(m => m.ScheduledDate >= startDate && m.ScheduledDate <= endDate)
               .Select(m => m.VehicleId)
               .Distinct()
               .ToListAsync();

            var conflictingVehicleIds = reservedVehicleIds.Union(maintenanceVehicleIds).ToList();

            // Étape 3: Récupérer les véhicules disponibles du type donné
            return await _dbContext.Vehicles
                .Where(v => v.VehicleTypeId == vehicleTypeId)
                .Where(v => v.Status == "Available" || v.Status == "Reserved")
                .Where(v => !conflictingVehicleIds.Contains(v.VehicleId))
                .Include(v => v.VehicleType)
                .ToListAsync();
        }

        // =========================================================
        // 3. Opérations sur le cycle de vie
        // =========================================================

        public async Task UpdateStatusAsync(Guid reservationId, string newStatus)
        {
            var reservation = await _dbSet.FindAsync(reservationId);
            if (reservation != null)
            {
                reservation.Status = newStatus;
                // Note : SaveChangesAsync doit être appelé par l'UnitOfWork (CompleteAsync)
            }
        }

        public async Task UpdateActualDatesAsync(Guid reservationId, DateTime? actualStart, DateTime? actualEnd)
        {
            var reservation = await _dbSet.FindAsync(reservationId);
            if (reservation != null)
            {
                if (actualStart.HasValue)
                {
                    reservation.ActualStart = actualStart.Value;
                }
                if (actualEnd.HasValue)
                {
                    reservation.ActualEnd = actualEnd.Value;
                }
                // Note : SaveChangesAsync doit être appelé par l'UnitOfWork (CompleteAsync)
            }
        }
    }
}