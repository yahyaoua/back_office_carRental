using CarRental.Api.Data;
using CarRental.Api.Services;
using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Api.Services 
{
    public class StatsService : IStatsService
    {
        private readonly ApplicationDbContext _context;

        public StatsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Total Véhicules
        public async Task<int> GetTotalVehiclesAsync()
        {
            return await _context.Vehicles.CountAsync();
        }

        // 2. Véhicules Disponibles
        public async Task<int> GetAvailableVehiclesCountAsync()
        {
            var now = DateTime.UtcNow;

            // Un véhicule est DISPONIBLE s'il n'a AUCUNE réservation non-terminée qui chevauche MAINTENANT
            return await _context.Vehicles
                .Where(v => !v.Reservations.Any(r =>
                    r.RequestedStart < now &&          
                    r.RequestedEnd > now &&            
                    r.Status != "Completed" &&         
                    r.Status != "Cancelled" &&        
                    r.Status != "NoShow"
                ))
                // On inclut aussi une vérification du statut propre du véhicule (Maintenance, Hors Service, etc.)
                .Where(v => v.Status == "Available" || v.Status == "Reserved")
                .CountAsync();
        }

        // 3. Réservations en Cours
        public async Task<int> GetActiveReservationsCountAsync()
        {
            // Une réservation est "en cours" si son statut est Pending, Confirmed, ou Active,
            // exactement comme dans votre GetActiveReservationsAsync du Repository.

            return await _context.Reservations
                .CountAsync(r =>
                    r.Status == "Pending" ||
                    r.Status == "Confirmed" ||
                    r.Status == "Active"
                );
        }
    }
}