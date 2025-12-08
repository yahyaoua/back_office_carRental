// Dans CarRental.Desktop.WPF/MaintenanceManagementWindow.xaml.cs

using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarRental.Desktop.WPF
{
    public partial class MaintenanceManagementWindow : Window
    {
        // ==========================================================
        // CHAMPS ET PROPRIÉTÉS
        // ==========================================================

        // Simule le service client qui appelle les endpoints API
        private readonly IVehicleService _maintenanceService;

        // Stocke la maintenance en cours pour le véhicule sélectionné
        private Maintenance? _currentPendingMaintenance;

        // ObservableCollections pour la liaison de données XAML
        public ObservableCollection<Vehicle> AvailableVehicles { get; set; } = new ObservableCollection<Vehicle>();
        public ObservableCollection<Maintenance> MaintenanceHistory { get; set; } = new ObservableCollection<Maintenance>();

        // ==========================================================
        // CONSTRUCTEUR ET INITIALISATION
        // ==========================================================

        // CONSTRUCTEUR PRINCIPAL CORRIGÉ : Seul celui-ci est maintenu.
        // Il assure que _maintenanceService est toujours initialisé.
        public MaintenanceManagementWindow(IVehicleService maintenanceService)
        {
            InitializeComponent();
            _maintenanceService = maintenanceService;
            this.DataContext = this;
            this.Loaded += MaintenanceManagementWindow_Loaded;
            // Assurez-vous que le DatePicker avec x:Name="dpScheduledDate" existe dans votre XAML
            // dpScheduledDate.SelectedDate = DateTime.Today; // Déplacé à la fin pour la lisibilité

            // Correction potentielle si dpScheduledDate n'est pas encore initialisé
            if (dpScheduledDate != null)
            {
                dpScheduledDate.SelectedDate = DateTime.Today;
            }
        }

        // LE CONSTRUCTEUR SUIVANT EST SUPPRIMÉ CAR IL CAUSE UN 'NullReferenceException' :
        /*
        public MaintenanceManagementWindow(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            // Si cette ligne est exécutée, _maintenanceService reste null.
        }
        */

        private async void MaintenanceManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Vérification de nullité ajoutée pour éviter les plantages si l'injection a échoué
            if (_maintenanceService == null)
            {
                MessageBox.Show("Erreur de dépendance : Le service de maintenance n'a pas été initialisé.", "Erreur d'Initialisation", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            await LoadDataAsync();
        }

        // ==========================================================
        // GESTION DU CHARGEMENT DES DONNÉES
        // ==========================================================

        private async Task LoadDataAsync()
        {
            try
            {
                // 1. Charger tous les véhicules (pour la ComboBox)
                var vehicles = await _maintenanceService.GetAllVehiclesAsync();
                AvailableVehicles.Clear();

                // Si la liste est vide, cela signifie que soit la BDD est vide, soit le service ne retourne rien.
                if (vehicles != null && vehicles.Any())
                {
                    foreach (var vehicle in vehicles.OrderBy(v => v.PlateNumber))
                    {
                        AvailableVehicles.Add(vehicle);
                    }
                }
                else
                {
                    // Optionnel : Afficher un message si aucune donnée n'est trouvée
                    // MessageBox.Show("Aucun véhicule trouvé pour la gestion de maintenance.");
                }

                // 2. Charger l'historique des maintenances (pour le DataGrid)
                var history = await _maintenanceService.GetAllMaintenancesAsync();
                MaintenanceHistory.Clear();
                if (history != null)
                {
                    foreach (var maintenance in history)
                    {
                        MaintenanceHistory.Add(maintenance);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}", "Erreur de Connexion", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ... (Le reste du code de la logique métier CmbVehicle_SelectionChanged, BtnScheduleMaintenance_Click, etc., reste inchangé) ...

        /// <summary>
        /// Gère la sélection d'un véhicule pour afficher son statut et sa maintenance active.
        /// </summary>
        private async void CmbVehicle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbVehicle.SelectedItem is Vehicle selectedVehicle)
            {
                // Mettre à jour le statut du véhicule
                txtVehicleStatus.Text = selectedVehicle.Status;

                // Récupérer la maintenance en cours (Pending/Scheduled/InProgress)
                await UpdatePendingMaintenanceViewAsync(selectedVehicle.VehicleId);
            }
            else
            {
                // Réinitialiser si rien n'est sélectionné
                txtVehicleStatus.Text = "Non Sélectionné";
                txtCurrentMaintenanceId.Text = "Aucune";
                txtCurrentMaintenanceType.Text = "N/A";
                BtnMarkReady.IsEnabled = false;
                _currentPendingMaintenance = null;
            }
        }

        // ... (Autres méthodes non modifiées) ...

        private async Task UpdatePendingMaintenanceViewAsync(Guid vehicleId)
        {
            _currentPendingMaintenance = await _maintenanceService.GetCurrentPendingMaintenanceAsync(vehicleId);

            if (_currentPendingMaintenance != null)
            {
                txtCurrentMaintenanceId.Text = _currentPendingMaintenance.MaintenanceId.ToString().Substring(0, 8) + "...";
                txtCurrentMaintenanceType.Text = _currentPendingMaintenance.Type;
                txtCurrentMaintenanceId.Foreground = Brushes.Red;
                txtCurrentMaintenanceType.Foreground = Brushes.Red;
                BtnMarkReady.IsEnabled = true;
            }
            else
            {
                txtCurrentMaintenanceId.Text = "Aucune";
                txtCurrentMaintenanceType.Text = "N/A";
                txtCurrentMaintenanceId.Foreground = Brushes.Green;
                txtCurrentMaintenanceType.Foreground = Brushes.Green;
                BtnMarkReady.IsEnabled = false;
            }
        }

        private async void BtnScheduleMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if (cmbVehicle.SelectedItem is not Vehicle selectedVehicle)
            {
                MessageBox.Show("Veuillez sélectionner un véhicule.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateScheduleInput(out string type, out DateTime date, out string notes))
            {
                return;
            }

            try
            {
                bool success = await _maintenanceService.ScheduleMaintenanceAsync(
                    selectedVehicle.VehicleId,
                    date,
                    type,
                    notes,
                    Guid.Empty
                );

                if (success)
                {
                    MessageBox.Show("Maintenance planifiée et statut du véhicule mis à jour !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                    await UpdatePendingMaintenanceViewAsync(selectedVehicle.VehicleId);
                    cmbVehicle.SelectedItem = AvailableVehicles.FirstOrDefault(v => v.VehicleId == selectedVehicle.VehicleId);
                    txtMaintenanceType.Clear();
                    txtNotes.Clear();
                    dpScheduledDate.SelectedDate = DateTime.Today;
                }
                else
                {
                    MessageBox.Show("La planification a échoué. Le service a retourné une erreur.", "Échec", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur API : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnMarkReady_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPendingMaintenance == null)
            {
                MessageBox.Show("Aucune maintenance active à finaliser.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Confirmez-vous la finalisation de la maintenance '{_currentPendingMaintenance.Type}' (ID : {_currentPendingMaintenance.MaintenanceId.ToString().Substring(0, 8)}...) ? Le véhicule passera en statut 'Available'.", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = await _maintenanceService.MarkVehicleReadyAsync(
                        _currentPendingMaintenance.VehicleId,
                        _currentPendingMaintenance.MaintenanceId
                    );

                    if (success)
                    {
                        MessageBox.Show("Maintenance finalisée. Le véhicule est maintenant 'Available' !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                        await LoadDataAsync();

                        if (cmbVehicle.SelectedItem is Vehicle selectedVehicleAfterUpdate)
                        {
                            await UpdatePendingMaintenanceViewAsync(selectedVehicleAfterUpdate.VehicleId);
                            var updatedVehicle = AvailableVehicles.FirstOrDefault(v => v.VehicleId == selectedVehicleAfterUpdate.VehicleId);
                            if (updatedVehicle != null)
                            {
                                txtVehicleStatus.Text = updatedVehicle.Status;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("La finalisation a échoué. Le service a retourné une erreur.", "Échec", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur API : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateScheduleInput(out string type, out DateTime date, out string notes)
        {
            type = txtMaintenanceType.Text.Trim();
            notes = txtNotes.Text.Trim();
            date = dpScheduledDate.SelectedDate ?? DateTime.Today;

            if (string.IsNullOrWhiteSpace(type))
            {
                MessageBox.Show("Veuillez spécifier le type de maintenance.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (date < DateTime.Today)
            {
                MessageBox.Show("La date de planification ne peut pas être antérieure à la date du jour.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}