// Dans CarRental2.Core/Interfaces/ITariffRepository.cs

using CarRental2.Core.Entities;

namespace CarRental2.Core.Interfaces
{
    public interface ITariffRepository : IGenericRepository<Tariff>
    {
        // Aucune méthode spécifique n'est nécessaire pour le moment, 
        // car la logique métier est simple (CRUD).
    }
}