// Dans CarRental2.Core/Interfaces/IVehicleTypeRepository.cs

using CarRental2.Core.Entities;

namespace CarRental2.Core.Interfaces
{
    public interface IVehicleTypeRepository : IGenericRepository<VehicleType>
    {
        // Aucune méthode spécifique n'est requise pour le moment.
    }
}