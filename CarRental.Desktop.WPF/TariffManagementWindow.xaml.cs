using CarRental2.Core.Interfaces;
using CarRental2.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Globalization;
// Ajout de cet using pour le DataGridAutoGeneratingColumnEventArgs, 
// sinon l'accès à e.Column.Header peut causer des problèmes si vous utilisez DgvTariffs_AutoGeneratingColumn
using System.Windows.Controls.Primitives;

namespace CarRental.Desktop.WPF
{
    public partial class TariffManagementWindow : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        private Tariff? _selectedTariff;
        private readonly ObservableCollection<Tariff> _tariffs = new ObservableCollection<Tariff>();

        // Constructeur : Injection de Dépendances
        public TariffManagementWindow(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            this.Loaded += TariffManagementWindow_Loaded;

            // Liaison initiale du ItemsSource
            dgvTariffs.ItemsSource = _tariffs;
        }

        private async void TariffManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadVehicleTypesAsync(); // 1. Charger la liste des types (FK)
            await LoadTariffsAsync();      // 2. Charger les tarifs
        }

        // =======================================================
        // LECTURE (READ) : Chargement des Données
        // =======================================================
        private async Task LoadVehicleTypesAsync()
        {
            try
            {
                // Charger tous les types de véhicules pour le ComboBox
                var vehicleTypes = await _unitOfWork.VehicleTypes.GetAllAsync();

                cmbVehicleType.ItemsSource = vehicleTypes.ToList();

                if (cmbVehicleType.Items.Count > 0)
                {
                    // Sélectionne le premier élément par défaut pour éviter le null lors de l'ajout
                    cmbVehicleType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des types de véhicules: {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTariffsAsync()
        {
            try
            {
                // 🚨 CORRECTION CS1739 : Retrait du paramètre 'includeProperties'
                var tariffsFromDb = await _unitOfWork.Tariffs.GetAllAsync();

                _tariffs.Clear();
                foreach (var tariff in tariffsFromDb)
                {
                    _tariffs.Add(tariff);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des tarifs: {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =======================================================
        // CRÉATION (CREATE) : Ajout
        // =======================================================
        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;
            if (_selectedTariff != null)
            {
                MessageBox.Show("Veuillez effacer le formulaire avant d'ajouter.", "Opération Invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validation du ComboBox (Clé Étrangère)
            if (cmbVehicleType.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un type de véhicule pour ce tarif.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validation de la conversion décimale
            if (!decimal.TryParse(txtPricePerDay.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal pricePerDayValue))
            {
                MessageBox.Show("Veuillez entrer un prix par jour valide.", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newTariff = new Tariff
            {
                TariffId = Guid.NewGuid(),
                Description = txtTariffDescription.Text,
                PricePerDay = pricePerDayValue,
                // Utilisation de la clé étrangère du ComboBox
                VehicleTypeId = (Guid)cmbVehicleType.SelectedValue
            };

            await _unitOfWork.Tariffs.AddAsync(newTariff);
            await _unitOfWork.CompleteAsync();

            MessageBox.Show("Tarif ajouté avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadTariffsAsync();
            ClearForm();
        }

        // =======================================================
        // MISE À JOUR (UPDATE)
        // =======================================================
        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTariff == null)
            {
                MessageBox.Show("Veuillez sélectionner un tarif à modifier.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateInput()) return;

            // Validation du ComboBox (Clé Étrangère)
            if (cmbVehicleType.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un type de véhicule pour ce tarif.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validation de la conversion décimale
            if (!decimal.TryParse(txtPricePerDay.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal pricePerDayValue))
            {
                MessageBox.Show("Veuillez entrer un prix par jour valide.", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _selectedTariff.Description = txtTariffDescription.Text;
            _selectedTariff.PricePerDay = pricePerDayValue;
            // Utilisation de la clé étrangère du ComboBox
            _selectedTariff.VehicleTypeId = (Guid)cmbVehicleType.SelectedValue;

            _unitOfWork.Tariffs.Update(_selectedTariff);
            await _unitOfWork.CompleteAsync();

            MessageBox.Show("Tarif mis à jour avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadTariffsAsync();
            ClearForm();
        }

        // =======================================================
        // SUPPRESSION (DELETE)
        // =======================================================
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTariff == null)
            {
                MessageBox.Show("Veuillez sélectionner un tarif à supprimer.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer le tarif '{_selectedTariff.Description}' ?", "Confirmer Suppression", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Supprimer par ID
                    await _unitOfWork.Tariffs.DeleteAsync(_selectedTariff.TariffId);
                    await _unitOfWork.CompleteAsync();

                    MessageBox.Show("Tarif supprimé avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadTariffsAsync();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur de suppression. Assurez-vous qu'aucune réservation ou véhicule n'utilise ce tarif.\n{ex.Message}", "Erreur de Suppression", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // =======================================================
        // Logique de Saisie et Affichage
        // =======================================================

        private void DgvTariffs_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Cacher les propriétés de navigation
            if (e.PropertyName == "VehicleType" || e.PropertyName == "Reservations")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }

            // Rendre les en-têtes plus lisibles
            if (e.PropertyName == "TariffId") e.Column.Header = "ID Tarif";
            if (e.PropertyName == "Description") e.Column.Header = "Description";
            if (e.PropertyName == "PricePerDay")
            {
                e.Column.Header = "Prix/Jour";
                // Formater l'affichage de la colonne en monétaire
                // e.Column.HeaderStyle = ... (Non applicable ici, utiliser StringFormat)
                if (e.Column is DataGridTextColumn textColumn && textColumn.Binding is System.Windows.Data.Binding binding)
                {
                    binding.StringFormat = "C2";
                }
            }
            if (e.PropertyName == "VehicleTypeId") e.Column.Header = "ID Type (FK)";

        }

        private void DgvTariffs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvTariffs.SelectedItem is Tariff tariff)
            {
                _selectedTariff = tariff;
                // Utiliser InvariantCulture pour s'assurer que le point décimal est correctement affiché (sinon la virgule pourrait poser problème)
                txtPricePerDay.Text = tariff.PricePerDay.ToString(CultureInfo.InvariantCulture);
                txtTariffDescription.Text = tariff.Description;

                // SÉLECTION DU BON ÉLÉMENT DANS LE COMBOBOX
                cmbVehicleType.SelectedValue = tariff.VehicleTypeId;
                return;
            }

            if (dgvTariffs.SelectedItem == null)
            {
                _selectedTariff = null;
                ClearForm(false);
            }
        }

        private void ClearForm(bool clearSelection = true)
        {
            txtTariffDescription.Text = string.Empty;
            txtPricePerDay.Text = string.Empty;
            // Réinitialiser le ComboBox au premier élément si possible
            if (cmbVehicleType.Items.Count > 0)
            {
                cmbVehicleType.SelectedIndex = 0;
            }
            else
            {
                cmbVehicleType.SelectedItem = null;
            }

            if (clearSelection)
            {
                _selectedTariff = null;
                dgvTariffs.SelectedItem = null;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm(true);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTariffDescription.Text) ||
                string.IsNullOrWhiteSpace(txtPricePerDay.Text))
            {
                MessageBox.Show("La Description du Tarif et le Prix par Jour sont requis.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
    }
}