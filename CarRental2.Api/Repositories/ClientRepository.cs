// Dans CarRental.Api/Repositories/ClientRepository.cs

using CarRental.Api.Data;
using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Api.Repositories
{
    // Hérite du GenericRepository pour le CRUD de base
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        private readonly ApplicationDbContext _appContext;

        public ClientRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
            _appContext = dbContext;
        }

        // ========================================================
        // 1. Implémentation de GetClientByEmailAsync (Connexion/Unicité)
        // ========================================================

        public async Task<Client> GetClientByEmailAsync(string email)
        {
            // Recherche de client par email, ignorant la casse.
            return await _appContext.Clients
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());
        }

        // ========================================================
        // 2. Implémentation de IsDriverLicenseNumberUniqueAsync (Validation)
        // ========================================================

        public async Task<bool> IsDriverLicenseNumberUniqueAsync(string licenseNumber)
        {
            // Vérifie s'il existe déjà un client avec ce numéro de permis.
            // Utilise Any() pour une vérification rapide (optimisation SQL)
            return !await _appContext.Clients
                .AnyAsync(c => c.DriverLicenseNumber.ToLower() == licenseNumber.ToLower());
        }

        // ========================================================
        // 3. Implémentation de GetClientWithReservationsAsync (Historique)
        // ========================================================

        public async Task<Client> GetClientWithReservationsAsync(Guid clientId)
        {
            // Récupère le client et inclut sa liste de réservations (Eager Loading)
            return await _appContext.Clients
                .Where(c => c.ClientId == clientId)
                .Include(c => c.Reservations)
                    .ThenInclude(r => r.Vehicle) // Inclure le véhicule de la réservation
                .FirstOrDefaultAsync();
        }

        // ========================================================
        // 4. Implémentation de GetClientByDriverLicenseNumberAsync
        // ========================================================

        public async Task<Client> GetClientByDriverLicenseNumberAsync(string licenseNumber)
        {
            // Recherche rapide par numéro de permis.
            return await _appContext.Clients
                .FirstOrDefaultAsync(c => c.DriverLicenseNumber.ToLower() == licenseNumber.ToLower());
        }
    }
}