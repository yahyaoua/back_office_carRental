using CarRental2.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Desktop.WPF.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly IStatsService _statsService;

        public DashboardViewModel(IStatsService statsService)
        {
            _statsService = statsService;
            // Initialisation des valeurs pour éviter l'affichage de "0" au démarrage
            // Elles seront mises à jour par LoadStatsAsync
            _totalVehicles = 0;
            _availableVehicles = 0;
            _activeReservations = 0;
        }

        // Implémentation de la propriété TotalVehicles
        private int _totalVehicles;
        public int TotalVehicles
        {
            get => _totalVehicles;
            set
            {
                if (_totalVehicles != value)
                {
                    _totalVehicles = value;
                    OnPropertyChanged();
                }
            }
        }

        // Implémentation de la propriété AvailableVehicles
        private int _availableVehicles;
        public int AvailableVehicles
        {
            get => _availableVehicles;
            set
            {
                if (_availableVehicles != value)
                {
                    _availableVehicles = value;
                    OnPropertyChanged();
                }
            }
        }

        // Implémentation de la propriété ActiveReservations
        private int _activeReservations;
        public int ActiveReservations
        {
            get => _activeReservations;
            set
            {
                if (_activeReservations != value)
                {
                    _activeReservations = value;
                    OnPropertyChanged();
                }
            }
        }

        
        public async Task LoadStatsAsync()
        {
            try
            {
                // Appel des méthodes asynchrones du service de statistiques
                this.TotalVehicles = await _statsService.GetTotalVehiclesAsync();
                this.AvailableVehicles = await _statsService.GetAvailableVehiclesCountAsync();
                this.ActiveReservations = await _statsService.GetActiveReservationsCountAsync();
            }
            catch (Exception ex)
            {
                // Gestion de l'erreur: En production, loggez ceci ou affichez une notification
                System.Diagnostics.Debug.WriteLine($"Erreur de chargement des statistiques: {ex.Message}");
                // Vous pouvez laisser les valeurs à 0 ou définir un état d'erreur
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}