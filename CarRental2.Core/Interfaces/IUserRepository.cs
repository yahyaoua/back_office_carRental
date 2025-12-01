// Dans CarRental2.Core/Interfaces/IUserRepository.cs

using CarRental2.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental2.Core.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        // Méthode spécifique pour l'authentification (Back-office)
        Task<User> GetByUsernameAsync(string username);

        // Méthode spécifique pour vérifier les privilèges lors de la connexion
        Task<bool> IsUserInRoleAsync(Guid userId, string role);

        // Méthode spécifique pour obtenir les utilisateurs par rôle (pour le filtrage dans le Back-office)
        Task<IReadOnlyList<User>> GetUsersByRoleAsync(string role);

        // Optionnel : Réactiver ou désactiver un utilisateur
        void SetUserActiveStatus(Guid userId, bool isActive);
    }
}