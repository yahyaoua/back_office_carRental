// Dans CarRental.Api/Services/VehicleService.cs (VERSION FINALE)

using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
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

        public async Task<IReadOnlyList<Vehicle>> SearchAvailableVehiclesAsync(Guid vehicleTypeId, DateTime start, DateTime end)
        {
            return await _unitOfWork.Vehicles.GetAvailableVehiclesAsync(start, end, vehicleTypeId);
        }

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
                Notes = notes, // Utilisation de Notes qui existe
                Status = "Scheduled",
                // Suppression de CreatedByUserId et CreatedAt qui n'existent pas
            };

            await _unitOfWork.Maintenances.AddAsync(maintenance);

            vehicle.Status = "Maintenance";
            _unitOfWork.Vehicles.Update(vehicle);

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

        public async Task<bool> AddImageToVehicleAsync(Guid vehicleId, string imagePath, bool isPrimary)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
            if (vehicle == null) return false;

            if (isPrimary)
            {
                var existingImages = await _unitOfWork.VehicleImages.FindAsync(img => img.VehicleId == vehicleId && img.IsPrimary);
                foreach (var img in existingImages)
                {
                    img.IsPrimary = false;
                    _unitOfWork.VehicleImages.Update(img);
                }
            }

            var newImage = new VehicleImage
            {
                VehicleImageId = Guid.NewGuid(),
                VehicleId = vehicleId,
                ImagePath = imagePath,
                IsPrimary = isPrimary
            };
            await _unitOfWork.VehicleImages.AddAsync(newImage);

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<Vehicle> GetVehicleDetailsAsync(Guid vehicleId)
        {
            return await _unitOfWork.Vehicles.GetVehicleWithDetailsAsync(vehicleId);
        }
    }
}