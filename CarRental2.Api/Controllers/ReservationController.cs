// Dans CarRental.Api/Controllers/ReservationController.cs

using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Définit la route de base comme /api/reservation
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        // Le contrôleur requiert IReservationService par injection de dépendances
        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        // =========================================================
        // 1. POST: Créer une nouvelle réservation
        // Route: POST /api/reservation
        // =========================================================

        /// <summary>
        /// Crée une nouvelle réservation après avoir vérifié la disponibilité du véhicule.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] Reservation reservation)
        {
            if (!reservation.VehicleId.HasValue)
            {
                // Un véhicule doit être sélectionné ou attribué avant la création
                return BadRequest("Un VehicleId est requis pour la création de la réservation.");
            }

            // Appelle la logique métier complexe du Service
            var createdReservation = await _reservationService.CreateReservationAsync(reservation);

            if (createdReservation == null)
            {
                return Conflict(new { Message = "Véhicule non disponible, période non valide ou erreur de création." });
            }

            // Statut 201 Created
            return CreatedAtAction(nameof(GetReservationDetails), new { id = createdReservation.ReservationId }, createdReservation);
        }

        // =========================================================
        // 2. GET: Obtenir les détails d'une réservation
        // Route: GET /api/reservation/{id}
        // =========================================================

        /// <summary>
        /// Obtient les détails complets d'une réservation.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationDetails(Guid id)
        {
            // Le service gère l'inclusion des entités liées (Client, Véhicule)
            var reservation = await _reservationService.GetDetailsAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            return Ok(reservation);
        }

        // =========================================================
        // 3. PUT: Traitement de la prise en charge (Agent)
        // Route: PUT /api/reservation/{id}/pickup
        // =========================================================

        /// <summary>
        /// Enregistre la prise en charge du véhicule. Nécessite une autorisation d'agent.
        /// </summary>
        [HttpPut("{id}/pickup")]
        public async Task<IActionResult> ProcessPickup(Guid id)
        {
            // TODO: Remplacer l'ID fictif par la récupération de l'ID de l'employé via l'authentification (JWT)
            Guid currentUserId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF0123456789"); // ID d'agent fictif

            bool success = await _reservationService.ProcessPickupAsync(id, currentUserId);

            if (!success)
            {
                return BadRequest(new { Message = "Impossible de traiter la prise en charge. Statut de réservation incorrect." });
            }

            return NoContent(); // Statut 204
        }

        // =========================================================
        // 4. PUT: Traitement du retour (Agent)
        // Route: PUT /api/reservation/{id}/return
        // =========================================================

        /// <summary>
        /// Enregistre le retour du véhicule, met à jour le kilométrage et calcule les frais supplémentaires.
        /// </summary>
        [HttpPut("{id}/return")]
        public async Task<IActionResult> ProcessReturn(Guid id, [FromQuery] int finalMileage)
        {
            if (finalMileage <= 0)
            {
                return BadRequest("Le kilométrage final est requis et doit être positif.");
            }

            // TODO: Remplacer l'ID fictif par la récupération de l'ID de l'employé via l'authentification (JWT)
            Guid currentUserId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF0123456789"); // ID d'agent fictif

            var result = await _reservationService.ProcessReturnAsync(id, currentUserId, finalMileage);

            if (!result.Success)
            {
                return BadRequest(new { Message = "Impossible de traiter le retour. La réservation doit être 'Active'." });
            }

            return Ok(new
            {
                Message = "Retour traité avec succès.",
                ExtraCharges = result.ExtraCharges,
                TotalCharged = result.ExtraCharges > 0 ? $"Frais supplémentaires de {result.ExtraCharges:C} calculés." : "Aucun frais supplémentaire."
            });
        }

        // =========================================================
        // 5. DELETE: Annuler une réservation (Client ou Agent)
        // Route: DELETE /api/reservation/{id}
        // =========================================================

        /// <summary>
        /// Annule une réservation (avec gestion de la pénalité dans la BLL).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReservation(Guid id)
        {
            // TODO: Récupération de l'ID de l'utilisateur (Client ou Agent)
            Guid currentUserId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF0123456789");

            bool success = await _reservationService.CancelReservationAsync(id, currentUserId);

            if (!success)
            {
                return BadRequest(new { Message = "Impossible d'annuler cette réservation. Le statut ne le permet pas (déjà complétée ou annulée)." });
            }

            return NoContent(); // Statut 204
        }
    }
}