// Dans CarRental.Api/Controllers/VehicleController.cs

using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route de base : /api/vehicle
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // =========================================================
        // 1. GET: Rechercher les véhicules disponibles
        // Route: GET /api/vehicle/search?typeId={guid}&start={date}&end={date}
        // =========================================================

        /// <summary>
        /// Recherche les véhicules disponibles pour une période et un type donnés.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchAvailableVehicles(
            [FromQuery] Guid vehicleTypeId,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            if (start >= end || vehicleTypeId == Guid.Empty)
            {
                return BadRequest("Les dates de début/fin doivent être valides et un type de véhicule doit être spécifié.");
            }

            var availableVehicles = await _vehicleService.SearchAvailableVehiclesAsync(vehicleTypeId, start, end);

            if (availableVehicles == null || availableVehicles.Count == 0)
            {
                return NotFound(new { Message = "Aucun véhicule disponible pour cette période et ce type." });
            }

            return Ok(availableVehicles);
        }

        // =========================================================
        // 2. GET: Obtenir les détails d'un véhicule
        // Route: GET /api/vehicle/{id}
        // =========================================================

        /// <summary>
        /// Obtient les détails complets d'un véhicule (incluant les images et l'historique de maintenance).
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleDetails(Guid id)
        {
            var vehicle = await _vehicleService.GetVehicleDetailsAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(vehicle);
        }

        // =========================================================
        // 3. POST: Planifier une maintenance (Agent Back-office)
        // Route: POST /api/vehicle/{id}/schedule-maintenance
        // =========================================================

        // Modèle interne pour la requête de maintenance
        public class ScheduleMaintenanceModel
        {
            public DateTime Date { get; set; }
            public string Type { get; set; }
            public string Notes { get; set; }
        }

        /// <summary>
        /// Planifie un entretien pour un véhicule et change son statut en 'Maintenance'.
        /// </summary>
        [HttpPost("{id}/schedule-maintenance")]
        // [Authorize(Roles = "Manager, Agent")] serait approprié ici
        public async Task<IActionResult> ScheduleMaintenance(Guid id, [FromBody] ScheduleMaintenanceModel model)
        {
            // Simulation de l'utilisateur/Agent connecté
            Guid currentUserId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF0123456789"); // ID d'agent fictif

            bool success = await _vehicleService.ScheduleMaintenanceAsync(
                id,
                model.Date,
                model.Type,
                model.Notes,
                currentUserId // Cet ID est ignoré si la propriété CreatedByUserId n'existe pas dans Maintenance.cs
            );

            if (!success)
            {
                return NotFound(new { Message = "Véhicule non trouvé ou échec de la planification." });
            }

            return Ok(new { Message = "Maintenance planifiée et statut du véhicule mis à jour." });
        }

        // =========================================================
        // 4. PUT: Marquer un véhicule comme prêt (Agent Back-office)
        // Route: PUT /api/vehicle/{id}/ready
        // =========================================================

        /// <summary>
        /// Marque la maintenance comme terminée et rend le véhicule 'Available'.
        /// </summary>
        [HttpPut("{id}/ready")]
        // [Authorize(Roles = "Manager, Agent")]
        public async Task<IActionResult> MarkVehicleReady(Guid id, [FromQuery] Guid maintenanceId)
        {
            if (maintenanceId == Guid.Empty)
            {
                return BadRequest("L'identifiant de la maintenance est requis.");
            }

            bool success = await _vehicleService.MarkVehicleReadyAsync(id, maintenanceId);

            if (!success)
            {
                return BadRequest(new { Message = "Impossible de marquer le véhicule comme prêt. Vérifiez les IDs." });
            }

            return NoContent(); // Statut 204
        }
    }
}