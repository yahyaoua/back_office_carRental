// Dans CarRental.Api/Services/ReservationService.cs (VERSION FINALE)

using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Api.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReservationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<decimal> CalculateTotalAmountAsync(Guid vehicleTypeId, DateTime start, DateTime end)
        {
            if (start >= end) return 0;
            int totalDays = (int)Math.Ceiling((end - start).TotalDays);

            // Utilisation de PricePerDay (Tariff.cs) et application des dates de validité
            var validTariffs = await _unitOfWork.Tariffs.FindAsync(t =>
                t.VehicleTypeId == vehicleTypeId &&
                t.StartDate <= start &&
                t.EndDate >= end
            );

            // Hypothèse : Prendre le tarif avec le plus grand PricePerDay si plusieurs s'appliquent (ou le premier)
            var tariff = validTariffs.OrderByDescending(t => t.PricePerDay).FirstOrDefault();

            if (tariff == null)
            {
                // Si aucun tarif spécifique n'est trouvé, utiliser le tarif de base du véhicule
                var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(vehicleTypeId);
                return vehicleType?.Vehicles.FirstOrDefault()?.BaseRatePerDay * totalDays ?? 0;
            }

            return tariff.PricePerDay * totalDays;
        }

        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            bool isAvailable = await _unitOfWork.Reservations.IsVehicleAvailableAsync(
                reservation.VehicleId.Value,
                reservation.RequestedStart,
                reservation.RequestedEnd);

            if (!isAvailable) return null;

            if (reservation.TotalAmount == 0)
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(reservation.VehicleId.Value);
                if (vehicle == null) return null;

                reservation.TotalAmount = await CalculateTotalAmountAsync(
                    vehicle.VehicleTypeId,
                    reservation.RequestedStart,
                    reservation.RequestedEnd);
            }

            reservation.Status = "Confirmed";
            await _unitOfWork.Reservations.AddAsync(reservation);
            await _unitOfWork.CompleteAsync();

            return reservation;
        }

        public async Task<bool> ProcessPickupAsync(Guid reservationId, Guid userId)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId);
            if (reservation == null || reservation.Status != "Confirmed") return false;

            // Utilisation de CreatedByUserId qui existe dans Reservation
            reservation.ActualStart = DateTime.UtcNow;
            reservation.Status = "Active";
            reservation.CreatedByUserId = userId; // L'employé qui gère la remise

            _unitOfWork.Reservations.Update(reservation);

            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(reservation.VehicleId.Value);
            if (vehicle != null)
            {
                vehicle.Status = "Rented";
                _unitOfWork.Vehicles.Update(vehicle);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<(bool Success, decimal ExtraCharges)> ProcessReturnAsync(Guid reservationId, Guid userId, int finalMileage)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId);
            if (reservation == null || reservation.Status != "Active") return (false, 0);

            decimal extraCharges = 0;
            DateTime returnTime = DateTime.UtcNow;

            // Calcul des frais supplémentaires (basé sur BaseRatePerDay du véhicule)
            if (returnTime > reservation.RequestedEnd)
            {
                TimeSpan delay = returnTime - reservation.RequestedEnd;
                int delayedDays = (int)Math.Ceiling(delay.TotalHours / 24);

                var vehiclee = await _unitOfWork.Vehicles.GetByIdAsync(reservation.VehicleId.Value);
                if (vehiclee != null)
                {
                    // Pénalité = Jours de retard * 1.5 * BaseRatePerDay
                    extraCharges = delayedDays * vehiclee.BaseRatePerDay * 1.5m;
                }
            }

            reservation.ActualEnd = returnTime;
            reservation.Status = "Completed";
            reservation.TotalAmount += extraCharges;
            reservation.CreatedByUserId = userId; // L'employé qui gère le retour

            _unitOfWork.Reservations.Update(reservation);

            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(reservation.VehicleId.Value);
            if (vehicle != null)
            {
                vehicle.Status = "Available";
                vehicle.Mileage = finalMileage; // Mise à jour du kilométrage
                _unitOfWork.Vehicles.Update(vehicle);
            }

            await _unitOfWork.CompleteAsync();
            return (true, extraCharges);
        }

        public async Task<bool> CancelReservationAsync(Guid reservationId, Guid userId)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId);

            if (reservation == null || reservation.Status == "Completed" || reservation.Status == "Cancelled") return false;

            reservation.Status = "Cancelled";
            reservation.CreatedByUserId = userId; // L'employé qui annule
            _unitOfWork.Reservations.Update(reservation);

            if (reservation.VehicleId.HasValue)
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(reservation.VehicleId.Value);
                if (vehicle != null && vehicle.Status == "Reserved")
                {
                    vehicle.Status = "Available";
                    _unitOfWork.Vehicles.Update(vehicle);
                }
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<Reservation> GetDetailsAsync(Guid reservationId)
        {
            return await _unitOfWork.Reservations.GetReservationWithDetailsAsync(reservationId);
        }
    }
}