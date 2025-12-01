// Dans CarRental.Api/Services/AuthService.cs (VERSION ALIGNÉE SANS PasswordHash dans Client)

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

        // Hachage simple pour la démo (utilisé seulement pour User)
        private static string HashPassword(string password) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password + "CarRentalSecretSalt"));
        private static bool VerifyPassword(string providedPassword, string storedHash) => HashPassword(providedPassword) == storedHash;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =========================================================
        // 1. Opérations Client (SANS PasswordHash dans Client.cs)
        // =========================================================

        public async Task<(bool Success, string Message, Client Client)> RegisterClientAsync(Client client, string password)
        {
            // Vérification d'unicité de l'Email et du Permis...
            if (await _unitOfWork.Clients.GetClientByEmailAsync(client.Email) != null)
                return (false, "L'adresse email est déjà utilisée.", null);

            bool isLicenseUnique = await _unitOfWork.Clients.IsDriverLicenseNumberUniqueAsync(client.DriverLicenseNumber);
            if (!isLicenseUnique)
                return (false, "Le numéro de permis de conduire est déjà enregistré.", null);

            // CORRECTION: Nous ne stockons pas le mot de passe car la propriété n'existe pas.
            // Le paramètre 'password' est ignoré ici.
            // client.PasswordHash = HashPassword(password); // LIGNE SUPPRIMÉE
            client.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Clients.AddAsync(client);
            await _unitOfWork.CompleteAsync();

            return (true, "Inscription réussie. (Authentification par mot de passe non supportée)", client);
        }

        public async Task<Client> AuthenticateClientAsync(string email, string password)
        {
            var client = await _unitOfWork.Clients.GetClientByEmailAsync(email);

            // CORRECTION: La vérification de mot de passe est impossible sans 'PasswordHash'.
            // Un système d'authentification sans mot de passe (ex: lien magique) serait nécessaire.

            // Pour l'instant, cette méthode retourne null car l'authentification est non sécurisée.
            return null;
        }

        // =========================================================
        // 2. Opérations Employé (User) (AVEC PasswordHash dans User.cs)
        // =========================================================

        public async Task<(bool Success, string Message, User User)> RegisterUserAsync(User user, string password)
        {
            if (await _unitOfWork.Users.GetByUsernameAsync(user.Username) != null)
                return (false, "Le nom d'utilisateur est déjà utilisé.", null);

            // La propriété PasswordHash existe dans User.cs, donc le hachage est fait.
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

            if (VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }
    }
}