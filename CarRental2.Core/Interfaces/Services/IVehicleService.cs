// Dans CarRental2.Core/Interfaces/Services/IVehicleService.cs

using CarRental2.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental2.Core.Interfaces.Services
{
    public interface IVehicleService
    {
        /// <summary>
        /// Recherche les véhicules disponibles d'un certain type pour une période donnée.
        /// </summary>
        Task<IReadOnlyList<Vehicle>> SearchAvailableVehiclesAsync(Guid vehicleTypeId, DateTime start, DateTime end);

        /// <summary>
        /// Enregistre une nouvelle entrée de maintenance pour un véhicule.
        /// </summary>
        Task<bool> ScheduleMaintenanceAsync(Guid vehicleId, DateTime date, string type, string notes, Guid createdByUserId);

        /// <summary>
        /// Marque un véhicule comme prêt après une maintenance.
        /// </summary>
        Task<bool> MarkVehicleReadyAsync(Guid vehicleId, Guid maintenanceId);

        /// <summary>
        /// Ajoute une image à un véhicule et gère l'état d'image principale.
        /// </summary>
        Task<bool> AddImageToVehicleAsync(Guid vehicleId, string imagePath, bool isPrimary);

        /// <summary>
        /// Récupère un véhicule avec tous ses détails (images, maintenances).
        /// </summary>
        Task<Vehicle> GetVehicleDetailsAsync(Guid vehicleId);
    }
}