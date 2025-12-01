// Dans CarRental.Api/Repositories/UserRepository.cs

using CarRental.Api.Data;
using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Api.Repositories
{
    // Hérite du GenericRepository pour le CRUD de base
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _appContext;

        public UserRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
            _appContext = dbContext;
        }

        // ========================================================
        // 1. Implémentation de GetByUsernameAsync (Authentification)
        // ========================================================

        public async Task<User> GetByUsernameAsync(string username)
        {
            // Recherche de l'utilisateur par nom d'utilisateur (ignorer la casse si nécessaire)
            // Note: Nous utilisons _appContext directement pour des requêtes spécifiques
            return await _appContext.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        // ========================================================
        // 2. Implémentation de IsUserInRoleAsync (Autorisation/Privilèges)
        // ========================================================

        public async Task<bool> IsUserInRoleAsync(Guid userId, string role)
        {
            // Récupère l'utilisateur et vérifie si son champ Role correspond à la chaîne fournie
            var user = await _dbSet.FindAsync(userId);

            // Effectue la vérification de rôle
            // Note: Une comparaison sans tenir compte de la casse est souvent préférable pour les chaînes de rôles.
            return user != null && user.Role.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        // ========================================================
        // 3. Implémentation de GetUsersByRoleAsync (Filtrage)
        // ========================================================

        public async Task<IReadOnlyList<User>> GetUsersByRoleAsync(string role)
        {
            return await _dbSet
                .Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }

        // ========================================================
        // 4. Implémentation de SetUserActiveStatus (Gestion du cycle de vie)
        // ========================================================

        public void SetUserActiveStatus(Guid userId, bool isActive)
        {
            // Trouver l'utilisateur, modifier son statut, et marquer l'entité comme modifiée
            // Note: L'appel à CompleteAsync() par le UnitOfWork sera nécessaire après cet appel.
            var user = _dbSet.Find(userId);
            if (user != null)
            {
                user.IsActive = isActive;
                // Marquer explicitement l'entité comme modifiée pour s'assurer qu'EF Core la suit
                _appContext.Entry(user).State = EntityState.Modified;
            }
        }
    }
}