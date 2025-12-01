// Dans CarRental2.Core/Interfaces/IClientRepository.cs

using CarRental2.Core.Entities;
using System;
using System.Threading.Tasks;

namespace CarRental2.Core.Interfaces
{
    public interface IClientRepository : IGenericRepository<Client>
    {
        // Méthode pour la connexion ou la vérification d'unicité (par Email)
        Task<Client> GetClientByEmailAsync(string email);

        // Méthode pour vérifier l'unicité du permis de conduire (requis lors de la création de compte)
        Task<bool> IsDriverLicenseNumberUniqueAsync(string licenseNumber);

        // Méthode pour obtenir un client avec l'historique de ses réservations
        Task<Client> GetClientWithReservationsAsync(Guid clientId);

        // Méthode pour trouver rapidement un client par son numéro de permis
        Task<Client> GetClientByDriverLicenseNumberAsync(string licenseNumber);
    }
}
