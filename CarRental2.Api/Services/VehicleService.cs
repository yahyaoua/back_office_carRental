// Dans CarRental.Api/Services/VehicleService.cs (VERSION MISE À JOUR)

using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using CarRental2.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;

namespace CarRental.Api.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ... (Autres méthodes de recherche et détails) ...

        public async Task<IReadOnlyList<Vehicle>> SearchAvailableVehiclesAsync(Guid vehicleTypeId, DateTime start, DateTime end)
        {
            return await _unitOfWork.Vehicles.GetAvailableVehiclesAsync(start, end, vehicleTypeId);
        }

        /// <summary>
        /// Planifie une maintenance. Le statut du véhicule n'est changé que si la date est aujourd'hui ou passée.
        /// </summary>
        public async Task<bool> ScheduleMaintenanceAsync(Guid vehicleId, DateTime date, string type, string notes, Guid createdByUserId)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
            if (vehicle == null) return false;

            var maintenance = new Maintenance
            {
                MaintenanceId = Guid.NewGuid(),
                VehicleId = vehicleId,
                ScheduledDate = date,
                Type = type,
                Notes = notes,
                Status = "Scheduled", // La maintenance est toujours 'Scheduled' (planifiée)
                // Note: J'ai retiré CreatedByUserId car il n'existe pas dans la classe Maintenance que vous avez fournie.
            };

            // ***********************************************
            // CORRECTION DU PROBLÈME 2 : CHANGEMENT DE STATUT CONDITIONNEL
            // ***********************************************
            if (date.Date <= DateTime.Today.Date)
            {
                vehicle.Status = "Maintenance";
                _unitOfWork.Vehicles.Update(vehicle);
            }

            await _unitOfWork.Maintenances.AddAsync(maintenance);

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> MarkVehicleReadyAsync(Guid vehicleId, Guid maintenanceId)
        {
            var maintenance = await _unitOfWork.Maintenances.GetByIdAsync(maintenanceId);
            if (maintenance == null) return false;

            maintenance.Status = "Done";
            _unitOfWork.Maintenances.Update(maintenance);

            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
            if (vehicle != null)
            {
                vehicle.Status = "Available";
                _unitOfWork.Vehicles.Update(vehicle);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        // ***********************************************
        // NOUVEAU : MISSION 4 - Ajout de l'image
        // ***********************************************

        /// <summary>
        /// Récupère le chemin (ImagePath) de l'image principale du véhicule.
        /// </summary>
        public async Task<string> GetPrimaryVehicleImagePathAsync(Guid vehicleId)
        {
            // Fait appel à la nouvelle méthode du repository (IVehicleRepository)
            var primaryImage = await _unitOfWork.Vehicles.GetPrimaryImageAsync(vehicleId);

            // Retourne le chemin d'accès si l'image est trouvée
            return primaryImage?.ImagePath;
        }

        // ... (Fin de AddImageToVehicleAsync et autres méthodes) ...

        public async Task<bool> AddImageToVehicleAsync(Guid vehicleId, string imagePath, bool isPrimary)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
            if (vehicle == null) return false;

            if (isPrimary)
            {
                // CORRECTION 1 : Remplacement de FindAsync par GetAllAsync
                // Récupère toutes les images principales existantes pour ce véhicule.
                var existingImages = await _unitOfWork.VehicleImages.GetAllAsync(img => img.VehicleId == vehicleId && img.IsPrimary);

                foreach (var img in existingImages)
                {
                    img.IsPrimary = false;
                    _unitOfWork.VehicleImages.Update(img);
                }

                // CORRECTION 2 : Sauvegarde des changements des images existantes
                // Ceci est crucial pour rendre l'ancienne image non-primaire AVANT d'ajouter la nouvelle.
                await _unitOfWork.CompleteAsync();
            }

            // Création de la nouvelle image
            var newImage = new VehicleImage
            {
                VehicleImageId = Guid.NewGuid(),
                VehicleId = vehicleId,
                ImagePath = imagePath,
                IsPrimary = isPrimary
            };

            // Ajout de la nouvelle image
            await _unitOfWork.VehicleImages.AddAsync(newImage);

            // Sauvegarde de l'ajout de la nouvelle image
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<Vehicle> GetVehicleDetailsAsync(Guid vehicleId)
        {
            return await _unitOfWork.Vehicles.GetVehicleWithDetailsAsync(vehicleId);
        }

        public async Task<IReadOnlyList<Vehicle>> GetAllVehiclesAsync()
        {
            return await _unitOfWork.Vehicles.GetAllAsync();
        }

        public async Task<IReadOnlyList<Maintenance>> GetAllMaintenancesAsync()
        {
            return await _unitOfWork.Maintenances.GetAllMaintenanceRecordsAsync();
        }

        public async Task<Maintenance> GetCurrentPendingMaintenanceAsync(Guid vehicleId)
        {
            return await _unitOfWork.Maintenances.GetCurrentPendingMaintenanceAsync(vehicleId);
        }


        public async Task<bool> HasMaintenanceConflictAsync(Guid vehicleId, DateTime start, DateTime end)
        {
            // CORRECTION 1 : Remplacement de FindAsync par GetAllAsync
            var conflictingMaintenance = await _unitOfWork.Maintenances.GetAllAsync(m =>
                m.VehicleId == vehicleId &&
                m.Status == "Scheduled" &&
                // Logique de chevauchement :
                (m.ScheduledDate.Date <= end.Date && m.ScheduledDate.Date >= start.Date)
            );

            // Retourne vrai si au moins un conflit est trouvé
            return conflictingMaintenance.Any();
        }

        // pour notificatins
        public async Task<IEnumerable<Maintenance>> GetUpcomingMaintenancesAsync(int daysThreshold)
        {
            var targetDate = DateTime.Today.AddDays(daysThreshold);

            var allMaintenances = await _unitOfWork.Maintenances.GetAllAsync();

            return allMaintenances
                .Where(m =>
                    m.ScheduledDate >= DateTime.Today &&
                    m.ScheduledDate <= targetDate &&
                    m.Status != "Completed")
                .ToList();
        }
    }
}