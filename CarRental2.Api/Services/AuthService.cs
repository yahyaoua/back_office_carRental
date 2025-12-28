using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace CarRental.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        // ⚠️ Hash simple pour la démo (OK pour projet scolaire)
        private static string HashPassword(string password)
            => Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(password + "CarRentalSecretSalt"));

        private static bool VerifyPassword(string providedPassword, string storedHash)
            => HashPassword(providedPassword) == storedHash;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =========================================================
        // 1. CLIENT
        // =========================================================

        public async Task<(bool Success, string Message, Client Client)> RegisterClientAsync(
            Client client,
            string password)
        {
            if (await _unitOfWork.Clients.GetClientByEmailAsync(client.Email) != null)
                return (false, "L'adresse email est déjà utilisée.", null);

            bool isLicenseUnique =
                await _unitOfWork.Clients.IsDriverLicenseNumberUniqueAsync(client.DriverLicenseNumber);

            if (!isLicenseUnique)
                return (false, "Le numéro de permis de conduire est déjà enregistré.", null);

            // ✅ HASH DU MOT DE PASSE CLIENT
            client.PasswordHash = HashPassword(password);
            client.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Clients.AddAsync(client);
            await _unitOfWork.CompleteAsync();

            return (true, "Inscription client réussie.", client);
        }

        public async Task<Client> AuthenticateClientAsync(string email, string password)
        {
            var client = await _unitOfWork.Clients.GetClientByEmailAsync(email);

            if (client == null)
                return null;

            if (!VerifyPassword(password, client.PasswordHash))
                return null;

            return client;
        }

        // =========================================================
        // 2. USER (EMPLOYÉ)
        // =========================================================

        public async Task<(bool Success, string Message, User User)> RegisterUserAsync(
            User user,
            string password)
        {
            if (await _unitOfWork.Users.GetByUsernameAsync(user.Username) != null)
                return (false, "Le nom d'utilisateur est déjà utilisé.", null);

            user.PasswordHash = HashPassword(password);
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            return (true, "Compte utilisateur créé avec succès.", user);
        }

        public async Task<User> AuthenticateUserAsync(string username, string password)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);

            if (user == null || !user.IsActive)
                return null;

            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }
    }
}
