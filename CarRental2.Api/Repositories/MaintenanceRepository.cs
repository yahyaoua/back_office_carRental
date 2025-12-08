namespace CarRental.Api.Repositories;
// Dans CarRental.Api/Repositories/MaintenanceRepository.cs

using CarRental.Api.Data;
using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System; // Ajouté pour Guid
using System.Collections.Generic; // Ajouté pour IReadOnlyList
using System.Linq;
using System.Threading.Tasks;

// Hérite du GenericRepository pour les méthodes de base (Add, Update, Delete)
public class MaintenanceRepository : GenericRepository<Maintenance>, IMaintenanceRepository
{
    public MaintenanceRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    // IMPLÉMENTATION 1 : Historique pour le DataGrid (CORRIGÉ pour inclure Vehicle)
    public async Task<IReadOnlyList<Maintenance>> GetAllMaintenanceRecordsAsync()
    {
        // Nous incluons le véhicule pour pouvoir afficher l'immatriculation/modèle dans le DataGrid
        return await _dbSet
            .Include(m => m.Vehicle) // Correction du Problème 1
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();
    }

    // IMPLÉMENTATION 2 : Maintenance en cours pour la Synthèse WPF
    public async Task<Maintenance?> GetCurrentPendingMaintenanceAsync(Guid vehicleId)
    {
        // Cherche le premier enregistrement (le plus récent planifié) qui n'est pas terminé ou annulé.
        return await _dbSet
            .Where(m => m.VehicleId == vehicleId)
            .Where(m => m.Status == "Scheduled" || m.Status == "InProgress") // Statuts actifs
            .OrderByDescending(m => m.ScheduledDate)
            .FirstOrDefaultAsync();
    }
}