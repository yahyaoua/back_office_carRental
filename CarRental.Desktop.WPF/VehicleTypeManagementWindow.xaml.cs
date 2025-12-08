using CarRental2.Core.Interfaces;
using CarRental2.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace CarRental.Desktop.WPF
{
    public partial class VehicleTypeManagementWindow : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        // Utilisez une propriété pour le DataContext si possible, sinon conservez le champ privé.
        private VehicleType? _selectedVehicleType;

        public VehicleTypeManagementWindow(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            this.Loaded += VehicleTypeManagementWindow_Loaded;
            // Optionnel : Configurer le DataContext si vous utilisez le Binding XAML
            // this.DataContext = this; 
        }

        private async void VehicleTypeManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadVehicleTypesAsync();
        }

        // =======================================================
        // LECTURE (READ) : Chargement et Affichage des Données
        // =======================================================
        private async Task LoadVehicleTypesAsync()
        {
            try
            {
                var vehicleTypes = await _unitOfWork.VehicleTypes.GetAllAsync();

                // L'ItemsSource est mise à jour avec la liste
                dgvVehicleTypes.ItemsSource = vehicleTypes.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des types de véhicules: {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =======================================================
        // CRÉATION (CREATE) : Ajout
        // =======================================================
        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;
            if (_selectedVehicleType != null)
            {
                MessageBox.Show("Veuillez effacer le formulaire avant d'ajouter.", "Opération Invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newVehicleType = new VehicleType
            {
                VehicleTypeId = Guid.NewGuid(),
                Name = txtVehicleTypeName.Text,
                // Lier le nouveau champ Description
                Description = txtVehicleTypeDescription.Text,

                // Initialisation des collections (pour éviter les erreurs EF Core)
                Vehicles = new List<Vehicle>(),
                Tariffs = new List<Tariff>()
            };

            await _unitOfWork.VehicleTypes.AddAsync(newVehicleType);
            await _unitOfWork.CompleteAsync();

            MessageBox.Show("Type de véhicule ajouté avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadVehicleTypesAsync();
            ClearForm();
        }

        // =======================================================
        // MISE À JOUR (UPDATE)
        // =======================================================
        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVehicleType == null)
            {
                MessageBox.Show("Veuillez sélectionner un type de véhicule à modifier.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateInput()) return;

            // Mise à jour des propriétés
            _selectedVehicleType.Name = txtVehicleTypeName.Text;
            // Mise à jour de la nouvelle propriété Description
            _selectedVehicleType.Description = txtVehicleTypeDescription.Text;

            _unitOfWork.VehicleTypes.Update(_selectedVehicleType);
            await _unitOfWork.CompleteAsync();

            MessageBox.Show("Type de véhicule mis à jour avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadVehicleTypesAsync();
            ClearForm();
        }

        // =======================================================
        // SUPPRESSION (DELETE)
        // =======================================================
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVehicleType == null)
            {
                MessageBox.Show("Veuillez sélectionner un type de véhicule à supprimer.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer le type '{_selectedVehicleType.Name}' ?\nCeci pourrait échouer si des véhicules ou tarifs y sont liés.", "Confirmer Suppression", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _unitOfWork.VehicleTypes.DeleteAsync(_selectedVehicleType.VehicleTypeId);
                    await _unitOfWork.CompleteAsync();

                    MessageBox.Show("Type de véhicule supprimé avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadVehicleTypesAsync();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression. Assurez-vous qu'aucun véhicule ou tarif n'est lié à ce type.\n{ex.Message}", "Erreur de Suppression", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // =======================================================
        // Logique de Saisie et Affichage
        // =======================================================

        private void DgvVehicleTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvVehicleTypes.SelectedItem is VehicleType vehicleType)
            {
                _selectedVehicleType = vehicleType;
                txtVehicleTypeName.Text = vehicleType.Name;
                // Liaison du champ Description lors de la sélection
                txtVehicleTypeDescription.Text = vehicleType.Description;
                return;
            }

            if (dgvVehicleTypes.SelectedItem == null)
            {
                _selectedVehicleType = null;
                ClearForm(false);
            }
        }

        private void ClearForm(bool clearSelection = true)
        {
            txtVehicleTypeName.Text = string.Empty;
            // Effacer le champ Description
            txtVehicleTypeDescription.Text = string.Empty;

            if (clearSelection)
            {
                _selectedVehicleType = null;
                dgvVehicleTypes.SelectedItem = null;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm(true);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtVehicleTypeName.Text))
            {
                MessageBox.Show("Le nom du type de véhicule est requis.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        // =======================================================
        // Masquage des colonnes (Implémentation)
        // =======================================================
        private void DgvVehicleTypes_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // 1. Masquer les colonnes de navigation et les IDs
            if (e.PropertyName == "VehicleTypeId")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
            else if (e.PropertyName == "Vehicles" ||       // Collection de navigation
                     e.PropertyName == "Tariffs")          // Collection de navigation
            {
                e.Column.Visibility = Visibility.Collapsed;
            }

            // 2. Renommage pour la lisibilité
            else if (e.PropertyName == "Name")
            {
                e.Column.Header = "Nom du Type";
            }
            else if (e.PropertyName == "Description")
            {
                e.Column.Header = "Description du Type";
            }
        }
    }
}