using CarRental2.Core.Interfaces;
using CarRental2.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CarRental.Desktop.WPF
{
    public partial class ReservationCreationWindow : Window
    {
        private readonly IUnitOfWork _unitOfWork;

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
        }

        private async void ReservationCreationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
            // Définir les dates par défaut
            // NOTE: Assurez-vous que les DatePickers dans le XAML ont les noms `dpStartDate` et `dpEndDate`
            // pour que ces lignes compilent correctement.
            // Si ces noms ne sont pas encore dans le XAML, cette partie échouera à la compilation XAML.
            // En supposant que le XAML est à jour.
            // dpStartDate.SelectedDate = DateTime.Today;
            // dpEndDate.SelectedDate = DateTime.Today.AddDays(7);
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
                // 1. Chargement des Clients (pour le ComboBox Client)
                // NOTE: Assurez-vous que la classe Client a les propriétés LastName et FirstName.
                var clients = await _unitOfWork.Clients.GetAllAsync();
                AvailableClients.Clear();
                foreach (var client in clients.OrderBy(c => c.LastName))
                {
                    AvailableClients.Add(client);
                }

                // 2. Chargement des Véhicules (pour le ComboBox Véhicule)
                // NOTE: Assurez-vous que la classe Vehicle a les propriétés Make et Model.
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();
                AvailableVehicles.Clear();
                foreach (var vehicle in vehicles.OrderBy(v => v.Make).ThenBy(v => v.Model))
                {
                    AvailableVehicles.Add(vehicle);
                }

                // 3. Chargement des Réservations Actives (utilisation de RequestedEnd pour le filtre)
                // NOTE: Assurez-vous que la classe Reservation a les propriétés RequestedEnd, RequestedStart, et VehicleId.
                var reservations = await _unitOfWork.Reservations.GetAllAsync();
                ActiveReservations.Clear();

                // Utilisation de .ToList() avant le foreach pour éviter des problèmes de thread si la liste est modifiée.
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
        // CRÉATION (CREATE) : Nouvelle Réservation
        // =======================================================
        private async void BtnCreateReservation_Click(object sender, RoutedEventArgs e)
        {
            // NOTE: Assurez-vous que les contrôles XAML cmbClient, cmbVehicle, dpStartDate, dpEndDate, txtTotalCost et txtDepositAmount existent
            // et que le bouton est nommé BtnCreateReservation.

            if (!ValidateInput(out DateTime requestedStart, out DateTime requestedEnd, out decimal totalAmount, out decimal depositAmount)) return;

            var selectedClient = cmbClient.SelectedItem as Client;
            var selectedVehicle = cmbVehicle.SelectedItem as Vehicle;

            if (selectedClient == null || selectedVehicle == null)
            {
                MessageBox.Show("Veuillez sélectionner un Client et un Véhicule valides.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Vérification de disponibilité utilisant RequestedStart et RequestedEnd
            var isVehicleAvailable = ActiveReservations
                .Where(r => r.VehicleId == selectedVehicle.VehicleId)
                // Condition de chevauchement de deux intervalles de temps [A, B] et [C, D] : A < D ET C < B
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
                    VehicleId = selectedVehicle.VehicleId, // Guid est converti implicitement en Guid?
                    // MAPPING CORRECT
                    RequestedStart = requestedStart,
                    RequestedEnd = requestedEnd,
                    TotalAmount = totalAmount,
                    DepositAmount = depositAmount,
                    Status = "Confirmed", // Statut initial
                };

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

        /// <summary>
        /// Valide les champs de saisie du formulaire.
        /// </summary>
        private bool ValidateInput(out DateTime requestedStart, out DateTime requestedEnd, out decimal totalAmount, out decimal depositAmount)
        {
            // Les champs de DatePicker retournent des valeurs nullables, la conversion ?? DateTime.MinValue est sûre
            requestedStart = dpStartDate.SelectedDate ?? DateTime.MinValue;
            requestedEnd = dpEndDate.SelectedDate ?? DateTime.MinValue;
            totalAmount = 0;
            depositAmount = 0;

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

            if (requestedStart < DateTime.Today)
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
            // Utilisation de CultureInfo.CurrentCulture (ou InvariantCulture si nécessaire, mais CurrentCulture est plus adapté à l'utilisateur)
            if (!decimal.TryParse(txtTotalCost.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out totalAmount) || totalAmount <= 0)
            {
                MessageBox.Show("Veuillez entrer un Montant Total valide (montant décimal positif).", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Validation du DepositAmount (txtDepositAmount)
            if (!decimal.TryParse(txtDepositAmount.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out depositAmount) || depositAmount < 0)
            {
                MessageBox.Show("Veuillez entrer un Montant de Dépôt valide (montant décimal positif ou nul).", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }


            return true;
        }

        private void ClearForm()
        {
            // Réinitialisation des contrôles
            cmbClient.SelectedItem = null;
            cmbVehicle.SelectedItem = null;
            txtTotalCost.Text = string.Empty;
            txtDepositAmount.Text = string.Empty;
            dpStartDate.SelectedDate = DateTime.Today;
            dpEndDate.SelectedDate = DateTime.Today.AddDays(7);
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        // =======================================================
        // Calcul du coût automatique
        // =======================================================
        private void CalculateCost()
        {
            // NOTE: Le véhicule doit avoir une propriété 'BaseRatePerDay' pour que ce calcul fonctionne.
            // Si cette propriété n'existe pas, elle devra être dérivée du VehicleType ou Tariff associé.

            if (cmbVehicle.SelectedItem is Vehicle selectedVehicle &&
                dpStartDate.SelectedDate.HasValue &&
                dpEndDate.SelectedDate.HasValue &&
                dpEndDate.SelectedDate.Value > dpStartDate.SelectedDate.Value)
            {
                var days = (dpEndDate.SelectedDate.Value - dpStartDate.SelectedDate.Value).TotalDays;
                if (days > 0)
                {
                    // La ligne suivante suppose que l'entité Vehicle a une propriété BaseRatePerDay de type decimal.
                    // Si BaseRatePerDay n'existe pas, vous devez récupérer l'information du tarif.
                    // Pour l'instant, on utilise une valeur par défaut ou simulée si la propriété n'existe pas.
                    // Exemple de simulation (à remplacer par la logique réelle de tarif):
                    decimal dailyRate = 50.0M; // Valeur par défaut / de test

                    // Si le véhicule est chargé avec les propriétés de navigation, vous pouvez utiliser:
                    // decimal dailyRate = selectedVehicle.Tariff.PricePerDay; 

                    // Pour le moment, nous allons utiliser le champ BaseRatePerDay que vous avez probablement dans l'entité Vehicle
                    // Si vous rencontrez une erreur 'BaseRatePerDay' n'existe pas, nous devrons modifier le modèle.

                    decimal cost;
                    try
                    {
                        cost = selectedVehicle.BaseRatePerDay * (decimal)days;
                    }
                    catch (Exception)
                    {
                        // Fallback si BaseRatePerDay n'est pas une propriété valide ou n'est pas chargé
                        cost = dailyRate * (decimal)days;
                    }


                    txtTotalCost.Text = cost.ToString("F2", CultureInfo.CurrentCulture);

                    // Exemple de dépôt : 10% du coût total
                    decimal deposit = Math.Round(cost * 0.1M, 2);
                    txtDepositAmount.Text = deposit.ToString("F2", CultureInfo.CurrentCulture);
                }
            }
        }

        // Événements pour déclencher le calcul du coût
        private void CmbVehicle_SelectionChanged(object sender, SelectionChangedEventArgs e) => CalculateCost();
        private void DpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => CalculateCost();
    }
}