// Dans CarRental2.Core/Interfaces/IUnitOfWork.cs (VERSION CORRIGÉE)

using CarRental2.Core.Entities;
using System;
using System.Threading.Tasks;

namespace CarRental2.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories Spécifiques
        IVehicleRepository Vehicles { get; }
        IReservationRepository Reservations { get; }
        IClientRepository Clients { get; }
        IUserRepository Users { get; }

        // --- CORRECTION CLÉS (Utiliser les interfaces spécifiques que nous avons créées) ---
        ITariffRepository Tariffs { get; }
        IVehicleTypeRepository VehicleTypes { get; }
        // ---------------------------------------------------------------------------------

        // Repositories Génériques pour les entités plus simples
        IGenericRepository<Maintenance> Maintenances { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<VehicleImage> VehicleImages { get; }

        /// <summary>
        /// Sauvegarde toutes les modifications en base de données.
        /// </summary>
        Task<int> CompleteAsync();
    }
}