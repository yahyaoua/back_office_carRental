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
using System.IO;
using Microsoft.Win32;
// AJOUT NÉCESSAIRE pour la méthode Include() et le Eager Loading
using Microsoft.EntityFrameworkCore;
using System.Windows.Data; // Pour le Binding dans AutoGeneratingColumn

// NOTE: La classe 'AppSettings' doit exister dans votre projet pour la gestion des chemins.
// namespace CarRental.Desktop.WPF { public static class AppSettings { public static string BaseImagePath = Path.Combine(Environment.CurrentDirectory, "VehicleImages"); } } 

namespace CarRental.Desktop.WPF
{
    public partial class VehicleManagementWindow : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        private Vehicle? _selectedVehicle;
        private readonly ObservableCollection<Vehicle> _vehicles = new ObservableCollection<Vehicle>();

        private readonly List<string> VehicleStatuses = new List<string> { "Available", "Rented", "Maintenance", "Reserved" };

        private readonly ObservableCollection<VehicleImage> _currentVehicleImages = new ObservableCollection<VehicleImage>();

        public VehicleManagementWindow(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            this.Loaded += VehicleManagementWindow_Loaded;

            dgvVehicles.ItemsSource = _vehicles;
            // Liaison de la ListView pour afficher les miniatures
            if (lstImages != null) lstImages.ItemsSource = _currentVehicleImages;
        }

        private async void VehicleManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadConfigurationDataAsync();
            await LoadVehiclesAsync();
        }

        private async Task LoadConfigurationDataAsync()
        {
            try
            {
                var vehicleTypes = await _unitOfWork.VehicleTypes.GetAllAsync();
                cmbVehicleType.ItemsSource = vehicleTypes.ToList();
                cmbStatus.ItemsSource = VehicleStatuses;

                if (cmbVehicleType.Items.Count > 0) cmbVehicleType.SelectedIndex = 0;
                if (cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0;

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
                // Utilisation de l'Eager Loading pour charger les relations (VehicleType et Images)
                var vehiclesFromDb = await _unitOfWork.Vehicles.GetAllAsync(
                    filter: null,
                    include: query => query
                        .Include(v => v.VehicleType)
                        .Include(v => v.Images)
                        .Include(v => v.Maintenances)
                        .Include(v => v.Reservations)
                );

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
        // GESTION DES IMAGES 
        // =======================================================

        private void BtnAddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png"
            };

            if (dlg.ShowDialog() == true)
            {
                string localPath = dlg.FileName;
                string fileName = Path.GetFileName(localPath);

                try
                {
                    // Assurez-vous que AppSettings.BaseImagePath est défini
                    // Exemple: string destinationDirectory = Path.Combine(Environment.CurrentDirectory, "VehicleImages");
                    string destinationDirectory = AppSettings.BaseImagePath;
                    Directory.CreateDirectory(destinationDirectory);

                    string finalStablePath = Path.Combine(destinationDirectory, fileName);

                    // Copier le fichier local vers le répertoire de destination
                    File.Copy(localPath, finalStablePath, true);

                    _currentVehicleImages.Add(new VehicleImage
                    {
                        VehicleImageId = Guid.NewGuid(),
                        ImagePath = finalStablePath,
                        // Définit la première image ajoutée comme Primaire
                        IsPrimary = !_currentVehicleImages.Any(i => i.IsPrimary)
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la copie du fichier. Détails: {ex.Message}", "Erreur Fichier", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // =======================================================
        // CRÉATION (CREATE) : Ajout
        // =======================================================
        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;
            if (!TryConvertInputs(out int year, out int mileage, out decimal baseRate, out DateTime maintenanceDate)) return;

            var newVehicle = new Vehicle
            {
                VehicleId = Guid.NewGuid(),
                PlateNumber = txtPlateNumber.Text,
                Make = txtMake.Text,
                Model = txtModel.Text,
                Year = year,
                Mileage = mileage,
                Status = cmbStatus.SelectedItem.ToString()!,
                BaseRatePerDay = baseRate,
                NextMaintenanceDate = maintenanceDate,
                VehicleTypeId = (Guid)cmbVehicleType.SelectedValue,
            };

            await _unitOfWork.Vehicles.AddAsync(newVehicle);
            await _unitOfWork.CompleteAsync();

            if (_currentVehicleImages.Any())
            {
                foreach (var image in _currentVehicleImages)
                {
                    image.VehicleId = newVehicle.VehicleId;
                    await _unitOfWork.VehicleImages.AddAsync(image);
                }
                await _unitOfWork.CompleteAsync();
            }

            MessageBox.Show("Véhicule et images ajoutés avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

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

            _selectedVehicle.PlateNumber = txtPlateNumber.Text;
            _selectedVehicle.Make = txtMake.Text;
            _selectedVehicle.Model = txtModel.Text;
            _selectedVehicle.Year = year;
            _selectedVehicle.Mileage = mileage;
            _selectedVehicle.Status = cmbStatus.SelectedItem.ToString()!;
            _selectedVehicle.BaseRatePerDay = baseRate;
            _selectedVehicle.NextMaintenanceDate = maintenanceDate;
            _selectedVehicle.VehicleTypeId = (Guid)cmbVehicleType.SelectedValue;

            _unitOfWork.Vehicles.Update(_selectedVehicle);
            // NOTE: La mise à jour des images est plus complexe et nécessiterait de gérer les ajouts/suppressions/modifications d'images existantes ici.
            // Pour l'instant, nous nous concentrons sur la mise à jour du véhicule lui-même.

            await _unitOfWork.CompleteAsync();

            MessageBox.Show("Véhicule mis à jour avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadVehiclesAsync();
            ClearForm();
        }

        // =======================================================
        // SUPPRESSION (DELETE) : Ajout de la logique de suppression en cascade manuelle
        // =======================================================
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVehicle == null || MessageBox.Show("Êtes-vous sûr de vouloir supprimer ce véhicule ? Cela supprimera toutes les images associées. Assurez-vous qu'il n'y a pas de réservations actives.", "Confirmer la Suppression", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                // Suppression des entités dépendantes (Images)
                var imagesToDelete = await _unitOfWork.VehicleImages.GetAllAsync(i => i.VehicleId == _selectedVehicle.VehicleId);
                foreach (var img in imagesToDelete)
                {
                    // Optionnel : Supprimer le fichier physique si vous le souhaitez
                    // if (File.Exists(img.ImagePath)) File.Delete(img.ImagePath);
                    _unitOfWork.VehicleImages.Delete(img);
                }

                _unitOfWork.Vehicles.Delete(_selectedVehicle);
                await _unitOfWork.CompleteAsync();

                MessageBox.Show("Véhicule supprimé avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadVehiclesAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression. Veuillez supprimer les réservations ou maintenances associées d'abord. Détails: {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // =======================================================
        // Logique de Saisie et Affichage
        // =======================================================

        private async void DgvVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvVehicles.SelectedItem is Vehicle vehicle)
            {
                _selectedVehicle = vehicle;

                // Remplissage du formulaire
                txtPlateNumber.Text = vehicle.PlateNumber;
                txtMake.Text = vehicle.Make;
                txtModel.Text = vehicle.Model;
                txtYear.Text = vehicle.Year.ToString();
                txtMileage.Text = vehicle.Mileage.ToString();
                txtBaseRatePerDay.Text = vehicle.BaseRatePerDay.ToString(CultureInfo.CurrentCulture);
                cmbStatus.SelectedItem = vehicle.Status;
                dpNextMaintenanceDate.SelectedDate = vehicle.NextMaintenanceDate;
                if (vehicle.VehicleTypeId != Guid.Empty)
                {
                    cmbVehicleType.SelectedValue = vehicle.VehicleTypeId;
                }

                _currentVehicleImages.Clear();

                // Afficher les images chargées via Eager Loading
                if (vehicle.Images != null && vehicle.Images.Any())
                {
                    foreach (var img in vehicle.Images)
                    {
                        _currentVehicleImages.Add(img);
                    }
                }
                else
                {
                    // Charger séparément si l'Eager Loading n'a pas été fait ou si vous devez forcer le chargement
                    try
                    {
                        var images = await _unitOfWork.VehicleImages.GetAllAsync(i => i.VehicleId == vehicle.VehicleId);
                        foreach (var img in images)
                        {
                            _currentVehicleImages.Add(img);
                        }
                    }
                    catch (Exception) { /* Gérer l'erreur de chargement d'images */ }
                }

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

            // Réinitialiser les combobox (optionnel)
            cmbVehicleType.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            dpNextMaintenanceDate.SelectedDate = DateTime.Today.AddDays(1);


            _currentVehicleImages.Clear();

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

        // =======================================================
        // VALIDATION ET CONVERSION
        // =======================================================

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtPlateNumber.Text))
            {
                MessageBox.Show("Le numéro de plaque est requis.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (cmbVehicleType.SelectedValue == null)
            {
                MessageBox.Show("Le type de véhicule est requis.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private bool TryConvertInputs(out int year, out int mileage, out decimal baseRate, out DateTime maintenanceDate)
        {
            year = 0;
            mileage = 0;
            baseRate = 0;
            maintenanceDate = dpNextMaintenanceDate.SelectedDate ?? DateTime.Today.AddYears(1);

            if (!int.TryParse(txtYear.Text, out year) || year < 1900 || year > DateTime.Now.Year + 2)
            {
                MessageBox.Show("Veuillez entrer une année de fabrication valide.", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(txtMileage.Text, out mileage) || mileage < 0)
            {
                MessageBox.Show("Veuillez entrer un kilométrage valide (nombre entier positif).", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!decimal.TryParse(txtBaseRatePerDay.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out baseRate) || baseRate <= 0)
            {
                MessageBox.Show("Veuillez entrer un Taux Journalier de Base valide et positif (ex: 50.00).", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            maintenanceDate = dpNextMaintenanceDate.SelectedDate ?? DateTime.Today.AddYears(1);

            return true;
        }

        // =======================================================
        // PERSONNALISATION DU DATAGRID (Pour afficher les noms et les comptes)
        // =======================================================
        private void DgvVehicles_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Cacher les clés primaires, étrangères et les collections d'entités de navigation
            if (e.PropertyName == "VehicleId" ||
                e.PropertyName == "VehicleTypeId" ||
                e.PropertyName == "Maintenances" ||
                e.PropertyName == "Reservations")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }

            // Renommage et formatage pour la lisibilité
            if (e.PropertyName == "PlateNumber")
            {
                e.Column.Header = "Plaque";
            }
            else if (e.PropertyName == "BaseRatePerDay")
            {
                e.Column.Header = "Tarif (Jour)";
                // Appliquer le format monétaire
                if (e.Column is DataGridTextColumn textColumn)
                {
                    textColumn.Binding.StringFormat = "C";
                }
            }
            else if (e.PropertyName == "VehicleType")
            {
                // Affiche le nom du VehicleType (grâce à l'Eager Loading)
                e.Column.Header = "Type";
                e.Column = new DataGridTextColumn
                {
                    Header = "Type",
                    Binding = new Binding("VehicleType.Name")
                };
            }
            else if (e.PropertyName == "Images")
            {
                // Affiche le nombre d'images (grâce à l'Eager Loading)
                e.Column.Header = "Nb. Photos";
                e.Column = new DataGridTextColumn
                {
                    Header = "Nb. Photos",
                    Binding = new Binding("Images.Count")
                };
            }
        }

        private async void lstImages_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;

            if (listView != null && listView.SelectedItem is VehicleImage selectedImage)
            {
                // 1. Désélectionner immédiatement l'élément 
                listView.SelectedItem = null;

                if (MessageBox.Show($"Voulez-vous supprimer l'image : {Path.GetFileName(selectedImage.ImagePath)} ?",
                                    "Confirmer la suppression",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Suppression dans la base de données
                        if (selectedImage.VehicleId != Guid.Empty)
                        {
                            _unitOfWork.VehicleImages.Delete(selectedImage);
                            await _unitOfWork.CompleteAsync(); // L'await est maintenant valide
                        }

                        // Suppression de la collection locale
                        _currentVehicleImages.Remove(selectedImage);

                        // Gérer le statut 'IsPrimary' 
                        if (selectedImage.IsPrimary && _currentVehicleImages.Any())
                        {
                            // Définir la première image restante comme Primaire
                            _currentVehicleImages.First().IsPrimary = true;
                        }

                        MessageBox.Show("Image supprimée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la suppression de l'image : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

    }

        
    }
