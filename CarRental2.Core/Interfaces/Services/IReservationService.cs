// Dans CarRental2.Core/Interfaces/Services/IReservationService.cs

using CarRental2.Core.Entities;
using System;
using System.Threading.Tasks;

namespace CarRental2.Core.Interfaces.Services
{
    public interface IReservationService
    {
        /// <summary>
        /// Calcule le montant total d'une location en fonction des dates, du type de véhicule et des tarifs.
        /// </summary>
        Task<decimal> CalculateTotalAmountAsync(Guid vehicleTypeId, DateTime start, DateTime end);

        /// <summary>
        /// Crée une nouvelle réservation après avoir vérifié la disponibilité et la validité des données.
        /// </summary>
        /// <param name="reservation">Objet Reservation contenant les informations initiales.</param>
        /// <returns>La réservation complète (avec un ID généré) ou null si indisponible.</returns>
        Task<Reservation> CreateReservationAsync(Reservation reservation);

        /// <summary>
        /// Traite le départ du véhicule : met à jour le statut, les dates réelles de départ, et peut générer un contrat.
        /// </summary>
        Task<bool> ProcessPickupAsync(Guid reservationId, Guid userId);

        /// <summary>
        /// Traite le retour du véhicule : calcule les frais supplémentaires (retard/dommages), met à jour le statut et les dates réelles de fin.
        /// </summary>
        Task<(bool Success, decimal ExtraCharges)> ProcessReturnAsync(Guid reservationId, Guid userId, int finalMileage);

        /// <summary>
        /// Annule une réservation et gère les potentiels remboursements ou pénalités (logique métier).
        /// </summary>
        Task<bool> CancelReservationAsync(Guid reservationId, Guid userId);

        /// <summary>
        /// Obtient le détail complet d'une réservation pour l'affichage du bon de réservation.
        /// </summary>
        Task<Reservation> GetDetailsAsync(Guid reservationId);
    }
}