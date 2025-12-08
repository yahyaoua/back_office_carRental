using CarRental2.Core.Interfaces;
using CarRental2.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data; // Nécessaire pour les DataGridTextColumn

namespace CarRental.Desktop.WPF
{
    public partial class ReservationCreationWindow : Window
    {
        private readonly IUnitOfWork _unitOfWork;

        // Stocke la réservation actuellement sélectionnée pour les opérations Modifier/Supprimer
        private Reservation _selectedReservation;

        // Collections Observables pour les liaisons de données
        public ObservableCollection<Client> AvailableClients { get; set; } = new ObservableCollection<Client>();
        public ObservableCollection<Vehicle> AvailableVehicles { get; set; } = new ObservableCollection<Vehicle>();
        public ObservableCollection<Reservation> ActiveReservations { get; set; } = new ObservableCollection<Reservation>();

        // Constructeur
        public ReservationCreationWindow(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            this.DataContext = this;
            this.Loaded += ReservationCreationWindow_Loaded;

            // Assurez-vous que le DataGrid existe et a les événements
            // dgvActiveReservations.SelectionChanged += DgvActiveReservations_SelectionChanged; 
            // dgvActiveReservations.AutoGeneratingColumn += DgvActiveReservations_AutoGeneratingColumn;
        }

        private async void ReservationCreationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
            ClearForm(); // Réinitialiser pour s'assurer que le mode est 'Création' initialement
        }

        // =======================================================
        // LECTURE (READ) : Chargement des Données Réelles
        // =======================================================

        /// <summary>
        /// Charge les listes réelles de Clients, de Véhicules et des Réservations actives.
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                // 1. Chargement des Clients
                var clients = await _unitOfWork.Clients.GetAllAsync();
                AvailableClients.Clear();
                foreach (var client in clients.OrderBy(c => c.LastName))
                {
                    AvailableClients.Add(client);
                }

                // 2. Chargement des Véhicules
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();
                AvailableVehicles.Clear();
                foreach (var vehicle in vehicles.OrderBy(v => v.Make).ThenBy(v => v.Model))
                {
                    AvailableVehicles.Add(vehicle);
                }

                // 3. Chargement des Réservations Actives (celles dont la date de fin demandée n'est pas passée)
                var reservations = await _unitOfWork.Reservations.GetAllAsync();
                ActiveReservations.Clear();
                foreach (var res in reservations.Where(r => r.RequestedEnd >= DateTime.Today).ToList())
                {
                    ActiveReservations.Add(res);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données de configuration : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =======================================================
        // FORMULAIRE ET CALCUL DU COÛT
        // =======================================================

        /// <summary>
        /// Valide les champs de saisie du formulaire (sans DepositAmount).
        /// </summary>
        private bool ValidateInput(out DateTime requestedStart, out DateTime requestedEnd, out decimal totalAmount)
        {
            requestedStart = dpStartDate.SelectedDate ?? DateTime.MinValue;
            requestedEnd = dpEndDate.SelectedDate ?? DateTime.MinValue;
            totalAmount = 0;

            if (cmbClient.SelectedItem == null || cmbVehicle.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un Client et un Véhicule.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (requestedStart == DateTime.MinValue || requestedEnd == DateTime.MinValue)
            {
                MessageBox.Show("Veuillez sélectionner des dates de début et de fin.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (requestedStart < DateTime.Today.Date)
            {
                MessageBox.Show("La date de début ne peut pas être dans le passé.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (requestedEnd <= requestedStart)
            {
                MessageBox.Show("La date de fin demandée doit être postérieure à la date de début.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Validation du TotalAmount (txtTotalCost)
            if (!decimal.TryParse(txtTotalCost.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out totalAmount) || totalAmount <= 0)
            {
                MessageBox.Show("Veuillez entrer un Montant Total valide (montant décimal positif).", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            _selectedReservation = null;
            cmbClient.SelectedItem = null;
            cmbVehicle.SelectedItem = null;
            txtTotalCost.Text = string.Empty;
            dpStartDate.SelectedDate = DateTime.Today;
            dpEndDate.SelectedDate = DateTime.Today.AddDays(7);
            BtnCreateReservation.IsEnabled = true;
            dgvActiveReservations.SelectedItem = null; // Désélectionner dans la grille
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void CalculateCost()
        {
            if (cmbVehicle.SelectedItem is Vehicle selectedVehicle &&
                dpStartDate.SelectedDate.HasValue &&
                dpEndDate.SelectedDate.HasValue &&
                dpEndDate.SelectedDate.Value > dpStartDate.SelectedDate.Value)
            {
                var days = (dpEndDate.SelectedDate.Value - dpStartDate.SelectedDate.Value).TotalDays;

                // Utilisation de la propriété BaseRatePerDay définie dans le XAML
                decimal dailyRate = selectedVehicle.BaseRatePerDay;

                if (days > 0)
                {
                    decimal cost = dailyRate * (decimal)days;
                    txtTotalCost.Text = cost.ToString("F2", CultureInfo.CurrentCulture);
                }
                else
                {
                    txtTotalCost.Text = "0.00";
                }
            }
            else
            {
                // Réinitialiser si les dates ou le véhicule ne sont pas valides
                txtTotalCost.Text = "0.00";
            }
        }

        private void CmbVehicle_SelectionChanged(object sender, SelectionChangedEventArgs e) => CalculateCost();
        private void DpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => CalculateCost();

        // =======================================================
        // DATA GRID (LECTURE & NETTOYAGE)
        // =======================================================

        /// <summary>
        /// Gère la sélection dans la grille pour charger les données dans le formulaire.
        /// </summary>
        private void DgvActiveReservations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Conversion explicite vers l'objet Reservation
            _selectedReservation = dgvActiveReservations.SelectedItem as Reservation;

            if (_selectedReservation != null)
            {
                // Remplir le formulaire avec les données sélectionnées
                cmbClient.SelectedValue = _selectedReservation.ClientId;
                cmbVehicle.SelectedValue = _selectedReservation.VehicleId;
                dpStartDate.SelectedDate = _selectedReservation.RequestedStart;
                dpEndDate.SelectedDate = _selectedReservation.RequestedEnd;
                txtTotalCost.Text = _selectedReservation.TotalAmount.ToString("F2", CultureInfo.CurrentCulture);

                // On passe en mode modification
                BtnCreateReservation.IsEnabled = false;
            }
            else
            {
                // Si la désélection se produit, on nettoie et on repasse en mode création
                ClearForm();
                BtnCreateReservation.IsEnabled = true;
            }
        }

        /// <summary>
        /// Cache les colonnes non pertinentes et formate les montants/dates.
        /// </summary>
        private void DgvActiveReservations_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Liste des propriétés à masquer (IDs longs et collections de navigation)
            if (e.PropertyName.EndsWith("Id") ||
                e.PropertyName == "ActualStart" ||
                e.PropertyName == "ActualEnd" ||
                e.PropertyName == "CreatedByUserId" ||
                e.PropertyName == "QRCodeData" ||
                e.PropertyName == "InvoicePath" ||
                e.PropertyName == "Client" ||
                e.PropertyName == "Vehicle" ||
                e.PropertyName == "CreatedByUser" ||
                e.PropertyName == "Payments")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }

            // Formatage des colonnes visibles
            else if (e.PropertyName == "RequestedStart")
            {
                e.Column.Header = "Début Dem.";
                (e.Column as DataGridTextColumn).Binding = new Binding(e.PropertyName) { StringFormat = "{0:dd/MM/yyyy}" };
            }
            else if (e.PropertyName == "RequestedEnd")
            {
                e.Column.Header = "Fin Dem.";
                (e.Column as DataGridTextColumn).Binding = new Binding(e.PropertyName) { StringFormat = "{0:dd/MM/yyyy}" };
            }
            else if (e.PropertyName == "TotalAmount")
            {
                e.Column.Header = "Total (€)";
                (e.Column as DataGridTextColumn).Binding = new Binding(e.PropertyName) { StringFormat = "{0:N2}" };
            }
            else if (e.PropertyName == "DepositAmount")
            {
                // Masque explicitement la colonne de dépôt
                e.Column.Visibility = Visibility.Collapsed;
            }
            else if (e.PropertyName == "Status")
            {
                e.Column.Header = "Statut";
            }
        }

        // =======================================================
        // CRÉATION (CREATE)
        // =======================================================

        private async void BtnCreateReservation_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput(out DateTime requestedStart, out DateTime requestedEnd, out decimal totalAmount)) return;

            var selectedClient = cmbClient.SelectedItem as Client;
            var selectedVehicle = cmbVehicle.SelectedItem as Vehicle;

            // ... (Vérification de disponibilité existante) ...
            var isVehicleAvailable = ActiveReservations
                .Where(r => r.VehicleId == selectedVehicle.VehicleId)
                .Any(r => requestedStart < r.RequestedEnd && requestedEnd > r.RequestedStart);

            if (isVehicleAvailable)
            {
                MessageBox.Show("Ce véhicule n'est pas disponible pour cette période demandée. Il est déjà réservé.", "Erreur de Disponibilité", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newReservation = new Reservation
                {
                    ReservationId = Guid.NewGuid(),
                    ClientId = selectedClient.ClientId,
                    VehicleId = selectedVehicle.VehicleId,
                    RequestedStart = requestedStart,
                    RequestedEnd = requestedEnd,
                    TotalAmount = totalAmount,
                    DepositAmount = 0, // Défini à 0 car le champ est retiré
                    Status = "Confirmed",
                };

                // Assurez-vous que IGenericRepository a bien AddAsync
                await _unitOfWork.Reservations.AddAsync(newReservation);
                await _unitOfWork.CompleteAsync();

                MessageBox.Show("Réservation créée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadDataAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création de la réservation : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =======================================================
        // MODIFICATION (UPDATE)
        // =======================================================

        private async void BtnUpdateReservation_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedReservation == null)
            {
                MessageBox.Show("Veuillez sélectionner une réservation à modifier dans la liste.", "Erreur de Sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInput(out DateTime requestedStart, out DateTime requestedEnd, out decimal totalAmount)) return;

            var selectedClient = cmbClient.SelectedItem as Client;
            var selectedVehicle = cmbVehicle.SelectedItem as Vehicle;

            // Vérification de disponibilité (excluant la réservation en cours de modification)
            var isVehicleAvailable = ActiveReservations
                .Where(r => r.VehicleId == selectedVehicle.VehicleId && r.ReservationId != _selectedReservation.ReservationId)
                .Any(r => requestedStart < r.RequestedEnd && requestedEnd > r.RequestedStart);

            if (isVehicleAvailable)
            {
                MessageBox.Show("Ce véhicule n'est pas disponible pour cette période demandée.", "Erreur de Disponibilité", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Mise à jour des propriétés
                _selectedReservation.ClientId = selectedClient.ClientId;
                _selectedReservation.VehicleId = selectedVehicle.VehicleId;
                _selectedReservation.RequestedStart = requestedStart;
                _selectedReservation.RequestedEnd = requestedEnd;
                _selectedReservation.TotalAmount = totalAmount;
                // DepositAmount n'est pas modifié ou est laissé à 0

                // Assurez-vous que IGenericRepository a bien Update
                _unitOfWork.Reservations.Update(_selectedReservation);
                await _unitOfWork.CompleteAsync();

                MessageBox.Show("Réservation modifiée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadDataAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la modification de la réservation : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =======================================================
        // SUPPRESSION (DELETE)
        // =======================================================

        private async void BtnDeleteReservation_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedReservation == null)
            {
                MessageBox.Show("Veuillez sélectionner une réservation à supprimer dans la liste.", "Erreur de Sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer la réservation n° {_selectedReservation.ReservationId.ToString().Substring(0, 8)}...?", "Confirmation de Suppression", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Assurez-vous que IGenericRepository a bien Remove
                    _unitOfWork.Reservations.Delete(_selectedReservation);
                    await _unitOfWork.CompleteAsync();

                    MessageBox.Show("Réservation supprimée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadDataAsync();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression de la réservation : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}