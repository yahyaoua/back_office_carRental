// Dans CarRental.Api/Controllers/AuthController.cs

using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarRental.Api.Controllers
{
    // Modèle de base pour la connexion (réutilisable pour Client et User)
    public class LoginModel
    {
        public string Identifier { get; set; } // Email pour Client, Username pour User
        public string Password { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")] // Route de base : /api/auth
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // =========================================================
        // 1. Enregistrement Client (Front-office)
        // Route: POST /api/auth/register/client
        // =========================================================

        /// <summary>
        /// Enregistre un nouveau client. Le mot de passe est transmis via le corps de l'objet Client.
        /// </summary>
        [HttpPost("register/client")]
        public async Task<IActionResult> RegisterClient([FromBody] Client client, [FromQuery] string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                return BadRequest("Le mot de passe est manquant ou trop court.");
            }

            var result = await _authService.RegisterClientAsync(client, password);

            if (!result.Success)
            {
                return Conflict(new { Message = result.Message });
            }

            // Statut 201 Created
            return CreatedAtAction(nameof(AuthenticateClient), new { email = client.Email }, result.Client);
        }

        // =========================================================
        // 2. Authentification Client (Front-office)
        // Route: POST /api/auth/login/client
        // =========================================================

        /// <summary>
        /// Authentifie un client via email et mot de passe.
        /// </summary>
        [HttpPost("login/client")]
        public async Task<IActionResult> AuthenticateClient([FromBody] LoginModel model)
        {
            var client = await _authService.AuthenticateClientAsync(model.Identifier, model.Password);

            if (client == null)
            {
                return Unauthorized(new { Message = "Email ou mot de passe client invalide." });
            }

            // NOTE: Ici, vous généreriez et retourneriez le Token JWT (JSON Web Token)
            return Ok(new
            {
                Message = "Connexion client réussie.",
                Client = client,
                Token = "GENERATED_JWT_TOKEN_HERE" // Placeholder
            });
        }

        // =========================================================
        // 3. Enregistrement Employé (Back-office)
        // Route: POST /api/auth/register/user
        // =========================================================

        /// <summary>
        /// Enregistre un nouvel utilisateur (employé).
        /// </summary>
        [HttpPost("register/user")]
        // [Authorize(Roles = "Admin")] serait nécessaire ici pour limiter qui peut créer des utilisateurs
        public async Task<IActionResult> RegisterUser([FromBody] User user, [FromQuery] string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                return BadRequest("Le mot de passe est manquant ou trop court.");
            }

            var result = await _authService.RegisterUserAsync(user, password);

            if (!result.Success)
            {
                return Conflict(new { Message = result.Message });
            }

            return CreatedAtAction(nameof(AuthenticateUser), new { username = user.Username }, result.User);
        }

        // =========================================================
        // 4. Authentification Employé (Back-office)
        // Route: POST /api/auth/login/user
        // =========================================================

        /// <summary>
        /// Authentifie un utilisateur (employé) via nom d'utilisateur et mot de passe.
        /// </summary>
        [HttpPost("login/user")]
        public async Task<IActionResult> AuthenticateUser([FromBody] LoginModel model)
        {
            var user = await _authService.AuthenticateUserAsync(model.Identifier, model.Password);

            if (user == null)
            {
                return Unauthorized(new { Message = "Nom d'utilisateur ou mot de passe employé invalide." });
            }

            // NOTE: Ici, vous généreriez et retourneriez le Token JWT (JSON Web Token)
            return Ok(new
            {
                Message = "Connexion employé réussie.",
                User = user,
                Token = "GENERATED_JWT_TOKEN_HERE" // Placeholder
            });
        }
    }
}
