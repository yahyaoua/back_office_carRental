// Dans CarRental2.Core/Interfaces/IReservationRepository.cs

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental2.Core.Entities;

namespace CarRental2.Core.Interfaces
{
    // Hérite de toutes les opérations CRUD de base pour Reservation
    public interface IReservationRepository : IGenericRepository<Reservation>
    {
        // =========================================================
        // 1. Méthodes de lecture avec détails (pour les interfaces)
        // =========================================================

        // Récupère une réservation avec toutes les entités liées (Client, Vehicle, Payments)
        Task<Reservation> GetReservationWithDetailsAsync(Guid reservationId);

        // Récupère toutes les réservations actives ou en cours pour une interface de suivi
        Task<IReadOnlyList<Reservation>> GetActiveReservationsAsync();

        // =========================================================
        // 2. Logique de Disponibilité (requête complexe)
        // =========================================================

        // Vérifie si le véhicule spécifique est disponible pendant une période donnée.
        // C'est l'implémentation qui devra vérifier les autres réservations et les maintenances.
        Task<bool> IsVehicleAvailableAsync(Guid vehicleId, DateTime startDate, DateTime endDate);

        // Récupère les véhicules d'un certain type qui ne sont pas loués ou en maintenance
        // durant la période spécifiée.
        // Cette méthode fait le lien entre la recherche client et la flotte.
        Task<IReadOnlyList<Vehicle>> GetAvailableVehiclesByTypeAsync(
            Guid vehicleTypeId,
            DateTime startDate,
            DateTime endDate);

        // =========================================================
        // 3. Opérations sur le cycle de vie
        // =========================================================

        // Met à jour le statut (Ex: Confirmed -> Active ou Active -> Completed)
        Task UpdateStatusAsync(Guid reservationId, string newStatus);

        // Met à jour les dates réelles de départ/retour
        Task UpdateActualDatesAsync(Guid reservationId, DateTime? actualStart, DateTime? actualEnd);
    }
}