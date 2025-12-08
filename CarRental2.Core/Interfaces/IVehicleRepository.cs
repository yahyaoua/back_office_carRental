// Dans CarRental2.Core/Interfaces/IVehicleRepository.cs

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental2.Core.Entities;

namespace CarRental2.Core.Interfaces
{
    // Hérite de toutes les opérations CRUD de base
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        // Ajout d'une méthode spécifique pour la logique métier de disponibilité
        Task<IReadOnlyList<Vehicle>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate, Guid vehicleTypeId);

        // Exemple : obtenir un véhicule avec ses images et son type
        Task<Vehicle> GetVehicleWithDetailsAsync(Guid vehicleId);
        Task<VehicleImage> GetPrimaryImageAsync(Guid vehicleId);
    }
}