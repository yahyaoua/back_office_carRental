// Dans CarRental2.Core/Interfaces/Services/IAuthService.cs

using CarRental2.Core.Entities;
using System.Threading.Tasks;

namespace CarRental2.Core.Interfaces.Services
{
    public interface IAuthService
    {
        // --- Opérations Client ---

        /// <summary>
        /// Enregistre un nouveau client après vérification des unicité (email, permis).
        /// </summary>
        Task<(bool Success, string Message, Client Client)> RegisterClientAsync(Client client, string password);

        /// <summary>
        /// Authentifie un client via son email et mot de passe.
        /// </summary>
        Task<Client> AuthenticateClientAsync(string email, string password);


        // --- Opérations Employé (User) ---

        /// <summary>
        /// Authentifie un utilisateur (employé) via nom d'utilisateur et mot de passe.
        /// </summary>
        Task<User> AuthenticateUserAsync(string username, string password);

        /// <summary>
        /// Crée un nouvel utilisateur (employé) dans le Back-office.
        /// </summary>
        Task<(bool Success, string Message, User User)> RegisterUserAsync(User user, string password);
    }
}