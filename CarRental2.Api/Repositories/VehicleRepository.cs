// Dans CarRental.Api/Repositories/VehicleRepository.cs

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
    // Hérite du GenericRepository pour les méthodes CRUD de base
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        // Le constructeur appelle le constructeur parent pour initialiser le DbContext et le DbSet
        public VehicleRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        // ========================================================
        // 1. Implémentation de GetAvailableVehiclesAsync
        //    (Logique de Disponibilité)
        // ========================================================

        public async Task<IReadOnlyList<Vehicle>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate, Guid vehicleTypeId)
        {
            // Étape 1: Identifier les ID de véhicules INDISPONIBLES à cause de RÉSERVATIONS
            var unavailableByReservationIds = await _dbContext.Reservations
                // Filtrer les réservations qui se chevauchent avec la période demandée
                .Where(r =>
                    (r.Status != "Completed" && r.Status != "Cancelled" && r.Status != "NoShow") && // Statuts en cours ou futurs
                    r.VehicleId.HasValue && // Un véhicule doit être attribué
                    (
                        // Critères de chevauchement temporels
                        (r.RequestedStart < endDate && r.RequestedEnd > startDate)
                    /*
                     * Le critère "r.RequestedStart < endDate && r.RequestedEnd > startDate" 
                     * est le moyen le plus simple de vérifier tout chevauchement.
                     */
                    )
                )
                .Select(r => r.VehicleId.Value)
                .Distinct()
                .ToListAsync();

            // Étape 2: Identifier les ID de véhicules INDISPONIBLES à cause de MAINTENANCE
            var unavailableByMaintenanceIds = await _dbContext.Maintenances
                // Filtrer les maintenances qui se chevauchent avec la période demandée
                .Where(m =>
                    m.Status != "Done" && m.Status != "Cancelled" && // Maintenance non terminée
                    (m.ScheduledDate >= startDate && m.ScheduledDate <= endDate.AddDays(-1)) // Simplification : maintenance dans la période
                )
                .Select(m => m.VehicleId)
                .Distinct()
                .ToListAsync();

            // Étape 3: Combiner les IDs indisponibles
            var conflictingVehicleIds = unavailableByReservationIds.Union(unavailableByMaintenanceIds).ToList();

            // Étape 4: Récupérer les véhicules qui ne sont pas en conflit
            return await _dbSet
                .Where(v => v.VehicleTypeId == vehicleTypeId) // Filtre par type demandé
                                                              // Exclure les véhicules qui ont un conflit dans la période
                .Where(v => !conflictingVehicleIds.Contains(v.VehicleId))
                // Exclure les véhicules dans un état non-louable (ex: accidenté, mis au rebut)
                .Where(v => v.Status == "Available" || v.Status == "Reserved" || v.Status == "Maintenance")
                // On inclut Maintenance pour laisser l'agent décider, mais le filtre de maintenance ci-dessus devrait le bloquer.
                // On peut s'assurer que seuls les véhicules "Available" sont louables: .Where(v => v.Status == "Available")
                .Include(v => v.VehicleType)
                .ToListAsync();
        }

        // ========================================================
        // 2. Implémentation de GetVehicleWithDetailsAsync
        //    (Lecture détaillée)
        // ========================================================

        public async Task<Vehicle> GetVehicleWithDetailsAsync(Guid vehicleId)
        {
            // Utilise Eager Loading pour inclure toutes les données liées en une seule requête
            return await _dbSet
                .Where(v => v.VehicleId == vehicleId)
                .Include(v => v.VehicleType)
                .Include(v => v.Images)
                .Include(v => v.Maintenances)
                .FirstOrDefaultAsync();
        }
        public async Task<VehicleImage> GetPrimaryImageAsync(Guid vehicleId)
        {
            // Nous utilisons le DbContext pour interroger directement la table VehicleImages
            // et nous cherchons la première image qui est marquée comme principale pour ce véhicule.
            return await _dbContext.VehicleImages
                .Where(img => img.VehicleId == vehicleId && img.IsPrimary)
                .FirstOrDefaultAsync(); // On prend le premier (et unique, en théorie)
        }
    }
}