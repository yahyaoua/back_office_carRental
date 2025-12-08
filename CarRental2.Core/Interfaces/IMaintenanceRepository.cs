// Dans CarRental2.Core/Interfaces/IMaintenanceRepository.cs

using CarRental2.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental2.Core.Interfaces
{
    // Hérite de toutes les opérations CRUD de base (GetByIdAsync, AddAsync, etc.)
    public interface IMaintenanceRepository : IGenericRepository<Maintenance>
    {
        // AJOUT 1: Récupère la liste complète des maintenances (pour le DataGrid)
        Task<IReadOnlyList<Maintenance>> GetAllMaintenanceRecordsAsync();

        // AJOUT 2: Récupère la maintenance non terminée pour un véhicule (pour la synthèse WPF)
        Task<Maintenance> GetCurrentPendingMaintenanceAsync(Guid vehicleId);
    }
}