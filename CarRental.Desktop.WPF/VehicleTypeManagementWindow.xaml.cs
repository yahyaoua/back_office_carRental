using System;
using System.Collections.Generic;
// Dans CarRental.Desktop.WPF/VehicleTypeManagementWindow.xaml.cs

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
        // Injection du IUnitOfWork pour accéder aux Repositories
        private readonly IUnitOfWork _unitOfWork;
        private VehicleType? _selectedVehicleType;

        // Constructeur : Injection de Dépendances
        public VehicleTypeManagementWindow(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            this.Loaded += VehicleTypeManagementWindow_Loaded;
        }

        private async void VehicleTypeManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Nous chargeons les types de véhicules en utilisant IUnitOfWork.VehicleTypes
            await LoadVehicleTypesAsync();
        }

        // =======================================================
        // LECTURE (READ) : Chargement et Affichage des Données
        // =======================================================
        private async Task LoadVehicleTypesAsync()
        {
            try
            {
                // NOTE : Assurez-vous que IUnitOfWork.VehicleTypes est correctement implémenté dans votre IUnitOfWork
                // et qu'il retourne IGenericRepository<VehicleType>.
                var vehicleTypes = await _unitOfWork.VehicleTypes.GetAllAsync();

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
                // Utilisation des propriétés de votre modèle : Name et Description
                Name = txtVehicleTypeName.Text,
                Description = "", // Description est souvent optionnelle à l'ajout rapide

                // Initialisation des collections (selon votre entité)
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

            // Mise à jour de la propriété Name
            _selectedVehicleType.Name = txtVehicleTypeName.Text;

            // Si vous ajoutez une zone de texte pour la Description plus tard, mettez-la à jour ici
            // _selectedVehicleType.Description = txtVehicleTypeDescription.Text; 

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
                    // Utilisation de la méthode DeleteAsync(Guid id)
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
                // Si vous avez un champ Description: txtVehicleTypeDescription.Text = vehicleType.Description;
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
            // Si vous avez un champ Description: txtVehicleTypeDescription.Text = string.Empty;
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
    }
}