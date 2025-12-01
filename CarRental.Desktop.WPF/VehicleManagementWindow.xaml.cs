using CarRental2.Core.Interfaces;
using CarRental2.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;

namespace CarRental.Desktop.WPF
{
    public partial class VehicleManagementWindow : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        private Vehicle? _selectedVehicle;
        private readonly ObservableCollection<Vehicle> _vehicles = new ObservableCollection<Vehicle>();

        // Liste des statuts possibles pour le ComboBox
        private readonly List<string> VehicleStatuses = new List<string> { "Available", "Rented", "Maintenance", "Reserved" };


        // Constructeur
        public VehicleManagementWindow(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            this.Loaded += VehicleManagementWindow_Loaded;

            dgvVehicles.ItemsSource = _vehicles;
        }

        private async void VehicleManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadConfigurationDataAsync(); // Charger Types de Véhicule et Statuts
            await LoadVehiclesAsync();    // Charger les Véhicules
        }

        // =======================================================
        // LECTURE (READ) : Chargement des Données
        // =======================================================

        /// <summary>
        /// Charge les Types de Véhicules (FK) et la liste des Statuts dans les ComboBox.
        /// </summary>
        private async Task LoadConfigurationDataAsync()
        {
            try
            {
                // 1. Chargement des Types de Véhicules (FK)
                var vehicleTypes = await _unitOfWork.VehicleTypes.GetAllAsync();
                cmbVehicleType.ItemsSource = vehicleTypes.ToList();

                // 2. Chargement des Statuts
                cmbStatus.ItemsSource = VehicleStatuses;

                // Pré-sélection du premier élément
                if (cmbVehicleType.Items.Count > 0) cmbVehicleType.SelectedIndex = 0;
                if (cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0;

                // Date par défaut : demain
                dpNextMaintenanceDate.SelectedDate = DateTime.Today.AddDays(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données de configuration : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadVehiclesAsync()
        {
            try
            {
                // Note: Si votre GetAllAsync supporte 'includeProperties', vous pouvez l'utiliser 
                // pour afficher le nom du type de véhicule au lieu de l'ID.
                var vehiclesFromDb = await _unitOfWork.Vehicles.GetAllAsync();

                _vehicles.Clear();
                foreach (var vehicle in vehiclesFromDb)
                {
                    _vehicles.Add(vehicle);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des véhicules : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =======================================================
        // CRÉATION (CREATE) : Ajout
        // =======================================================
        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;
            if (_selectedVehicle != null)
            {
                MessageBox.Show("Veuillez effacer le formulaire avant d'ajouter un nouveau véhicule.", "Opération Invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryConvertInputs(out int year, out int mileage, out decimal baseRate, out DateTime maintenanceDate)) return;

            // Validation des clés étrangères et du statut
            if (cmbVehicleType.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un Type de Véhicule.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un Statut.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            var newVehicle = new Vehicle
            {
                VehicleId = Guid.NewGuid(),
                PlateNumber = txtPlateNumber.Text, // ADAPTÉ
                Make = txtMake.Text,               // ADAPTÉ
                Model = txtModel.Text,
                Year = year,
                Mileage = mileage,                 // ADAPTÉ
                Status = cmbStatus.SelectedItem.ToString()!, // ADAPTÉ
                BaseRatePerDay = baseRate,         // ADAPTÉ
                NextMaintenanceDate = maintenanceDate, // ADAPTÉ
                VehicleTypeId = (Guid)cmbVehicleType.SelectedValue,
                // CreatedAt est défini par défaut dans l'entité
            };

            await _unitOfWork.Vehicles.AddAsync(newVehicle);
            await _unitOfWork.CompleteAsync();

            MessageBox.Show("Véhicule ajouté avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadVehiclesAsync();
            ClearForm();
        }

        // =======================================================
        // MISE À JOUR (UPDATE)
        // =======================================================
        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVehicle == null)
            {
                MessageBox.Show("Veuillez sélectionner un véhicule à modifier.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateInput()) return;

            if (!TryConvertInputs(out int year, out int mileage, out decimal baseRate, out DateTime maintenanceDate)) return;

            // Validation des clés étrangères et du statut
            if (cmbVehicleType.SelectedValue == null || cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un Type de Véhicule et un Statut.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Mise à jour des propriétés
            _selectedVehicle.PlateNumber = txtPlateNumber.Text; // ADAPTÉ
            _selectedVehicle.Make = txtMake.Text; // ADAPTÉ
            _selectedVehicle.Model = txtModel.Text;
            _selectedVehicle.Year = year;
            _selectedVehicle.Mileage = mileage; // ADAPTÉ
            _selectedVehicle.Status = cmbStatus.SelectedItem.ToString()!; // ADAPTÉ
            _selectedVehicle.BaseRatePerDay = baseRate; // ADAPTÉ
            _selectedVehicle.NextMaintenanceDate = maintenanceDate; // ADAPTÉ
            _selectedVehicle.VehicleTypeId = (Guid)cmbVehicleType.SelectedValue;

            _unitOfWork.Vehicles.Update(_selectedVehicle);
            await _unitOfWork.CompleteAsync();

            MessageBox.Show("Véhicule mis à jour avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadVehiclesAsync();
            ClearForm();
        }

        // =======================================================
        // SUPPRESSION (DELETE)
        // =======================================================
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVehicle == null)
            {
                MessageBox.Show("Veuillez sélectionner un véhicule à supprimer.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer le véhicule '{_selectedVehicle.PlateNumber}' ({_selectedVehicle.Make} {_selectedVehicle.Model}) ?", "Confirmer Suppression", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _unitOfWork.Vehicles.DeleteAsync(_selectedVehicle.VehicleId);
                    await _unitOfWork.CompleteAsync();

                    MessageBox.Show("Véhicule supprimé avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadVehiclesAsync();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur de suppression. Assurez-vous qu'aucune réservation n'est liée à ce véhicule.\n{ex.Message}", "Erreur de Suppression", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // =======================================================
        // Logique de Saisie et Affichage
        // =======================================================

        /// <summary>
        /// Personnalise les colonnes du DataGrid.
        /// </summary>
        private void DgvVehicles_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Cacher les propriétés de navigation (Collections)
            if (e.PropertyName == "VehicleType" ||
                e.PropertyName == "Images" ||
                e.PropertyName == "Maintenances" ||
                e.PropertyName == "Reservations")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }

            // Rendre les en-têtes plus lisibles
            if (e.PropertyName == "VehicleId") e.Column.Header = "ID Véhicule";
            if (e.PropertyName == "PlateNumber") e.Column.Header = "N° Plaque";
            if (e.PropertyName == "Make") e.Column.Header = "Marque";
            if (e.PropertyName == "Model") e.Column.Header = "Modèle";
            if (e.PropertyName == "Year") e.Column.Header = "Année";
            if (e.PropertyName == "Mileage") e.Column.Header = "Kilométrage (km)";
            if (e.PropertyName == "Status") e.Column.Header = "Statut";
            if (e.PropertyName == "BaseRatePerDay") e.Column.Header = "Prix Jour (€)";
            if (e.PropertyName == "NextMaintenanceDate") e.Column.Header = "Proch. Maintenance";
            if (e.PropertyName == "VehicleTypeId") e.Column.Header = "ID Type (FK)";
            if (e.PropertyName == "CreatedAt") e.Column.Header = "Créé le";
        }

        /// <summary>
        /// Charge le véhicule sélectionné du DataGrid dans les champs de saisie.
        /// </summary>
        private void DgvVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvVehicles.SelectedItem is Vehicle vehicle)
            {
                _selectedVehicle = vehicle;
                txtPlateNumber.Text = vehicle.PlateNumber;
                txtMake.Text = vehicle.Make;
                txtModel.Text = vehicle.Model;
                txtYear.Text = vehicle.Year.ToString();
                txtMileage.Text = vehicle.Mileage.ToString();
                txtBaseRatePerDay.Text = vehicle.BaseRatePerDay.ToString(CultureInfo.InvariantCulture); // Utiliser '.' comme séparateur décimal
                dpNextMaintenanceDate.SelectedDate = vehicle.NextMaintenanceDate;

                // SÉLECTION DES CLÉS ÉTRANGÈRES et du statut
                cmbVehicleType.SelectedValue = vehicle.VehicleTypeId;
                cmbStatus.SelectedItem = vehicle.Status;
                return;
            }

            if (dgvVehicles.SelectedItem == null)
            {
                _selectedVehicle = null;
                ClearForm(false);
            }
        }

        private void ClearForm(bool clearSelection = true)
        {
            txtPlateNumber.Text = string.Empty;
            txtMake.Text = string.Empty;
            txtModel.Text = string.Empty;
            txtYear.Text = string.Empty;
            txtMileage.Text = string.Empty;
            txtBaseRatePerDay.Text = string.Empty;

            // Réinitialiser les ComboBox et DatePicker
            if (cmbVehicleType.Items.Count > 0) cmbVehicleType.SelectedIndex = 0; else cmbVehicleType.SelectedItem = null;
            if (cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0; else cmbStatus.SelectedItem = null;
            dpNextMaintenanceDate.SelectedDate = DateTime.Today.AddDays(1); // Date par défaut

            if (clearSelection)
            {
                _selectedVehicle = null;
                dgvVehicles.SelectedItem = null;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm(true);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtPlateNumber.Text) ||
                string.IsNullOrWhiteSpace(txtMake.Text) ||
                string.IsNullOrWhiteSpace(txtModel.Text) ||
                string.IsNullOrWhiteSpace(txtYear.Text) ||
                string.IsNullOrWhiteSpace(txtMileage.Text) ||
                string.IsNullOrWhiteSpace(txtBaseRatePerDay.Text))
            {
                MessageBox.Show("Tous les champs de texte sont requis.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (dpNextMaintenanceDate.SelectedDate == null)
            {
                MessageBox.Show("La date de prochaine maintenance est requise.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tente de convertir les champs numériques et la date.
        /// </summary>
        private bool TryConvertInputs(out int year, out int mileage, out decimal baseRate, out DateTime maintenanceDate)
        {
            year = 0;
            mileage = 0;
            baseRate = 0;
            maintenanceDate = dpNextMaintenanceDate.SelectedDate ?? DateTime.Today;

            if (!int.TryParse(txtYear.Text, out year) || year <= 1900 || year > DateTime.Now.Year + 1)
            {
                MessageBox.Show("Veuillez entrer une année de fabrication valide (ex: 2022).", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(txtMileage.Text, out mileage) || mileage < 0)
            {
                MessageBox.Show("Veuillez entrer un kilométrage actuel valide (nombre entier positif).", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Utiliser InvariantCulture pour décimal, supportant le point ('.') ou la virgule (',') selon les paramètres régionaux par défaut
            if (!decimal.TryParse(txtBaseRatePerDay.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out baseRate) || baseRate < 0)
            {
                MessageBox.Show("Veuillez entrer un Prix Journalier de Base valide (montant décimal positif).", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}