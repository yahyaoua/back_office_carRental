// Dans CarRental.Api/Controllers/ManagementController.cs

using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Api.Controllers
{
    // [Authorize(Roles = "Admin, Manager")] // Cet attribut serait essentiel ici
    [ApiController]
    [Route("api/management")] // Route de base : /api/management
    public class ManagementController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ManagementController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =========================================================
        // A. Gestion des Types de Véhicules (VehicleType)
        // =========================================================

        /// <summary>
        /// Obtient tous les types de véhicules.
        /// </summary>
        [HttpGet("vehicletypes")]
        public async Task<ActionResult<IReadOnlyList<VehicleType>>> GetAllVehicleTypes()
        {
            var types = await _unitOfWork.VehicleTypes.GetAllAsync();
            return Ok(types);
        }

        /// <summary>
        /// Crée un nouveau type de véhicule.
        /// </summary>
        [HttpPost("vehicletypes")]
        public async Task<IActionResult> CreateVehicleType([FromBody] VehicleType vehicleType)
        {
            vehicleType.VehicleTypeId = Guid.NewGuid();
            await _unitOfWork.VehicleTypes.AddAsync(vehicleType);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetAllVehicleTypes), new { id = vehicleType.VehicleTypeId }, vehicleType);
        }

        // =========================================================
        // B. Gestion des Tarifs (Tariff)
        // =========================================================

        /// <summary>
        /// Obtient tous les tarifs.
        /// </summary>
        [HttpGet("tariffs")]
        public async Task<ActionResult<IReadOnlyList<Tariff>>> GetAllTariffs()
        {
            var tariffs = await _unitOfWork.Tariffs.GetAllAsync();
            return Ok(tariffs);
        }

        /// <summary>
        /// Crée une nouvelle règle tarifaire.
        /// </summary>
        [HttpPost("tariffs")]
        public async Task<IActionResult> CreateTariff([FromBody] Tariff tariff)
        {
            tariff.TariffId = Guid.NewGuid();
            await _unitOfWork.Tariffs.AddAsync(tariff);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetAllTariffs), new { id = tariff.TariffId }, tariff);
        }

        /// <summary>
        /// Supprime une règle tarifaire par ID.
        /// </summary>
        [HttpDelete("tariffs/{id}")]
        public async Task<IActionResult> DeleteTariff(Guid id)
        {
            var tariff = await _unitOfWork.Tariffs.GetByIdAsync(id);
            if (tariff == null) return NotFound();

            _unitOfWork.Tariffs.Delete(tariff);
            await _unitOfWork.CompleteAsync();

            return NoContent(); // Statut 204
        }

        // =========================================================
        // C. Gestion des Clients (CRUD simple)
        // =========================================================

        /// <summary>
        /// Obtient la liste de tous les clients.
        /// </summary>
        [HttpGet("clients")]
        public async Task<ActionResult<IReadOnlyList<Client>>> GetAllClients()
        {
            var clients = await _unitOfWork.Clients.GetAllAsync();
            return Ok(clients);
        }

        /// <summary>
        /// Met à jour les informations d'un client.
        /// </summary>
        [HttpPut("clients/{id}")]
        public async Task<IActionResult> UpdateClient(Guid id, [FromBody] Client client)
        {
            if (id != client.ClientId)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID du corps de la requête.");
            }

            // NOTE: Ceci est une mise à jour brute. Dans un cas réel, vous feriez une mise à jour partielle.
            _unitOfWork.Clients.Update(client);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }
}