using CarRental2.Core.Interfaces;
using CarRental2.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;


namespace CarRental.Desktop.WPF
{
    public partial class ClientManagementWindow : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        private Client? _selectedClient;
        private readonly ObservableCollection<Client> _clients = new ObservableCollection<Client>();

        // Constructeur : Injection de Dépendances
        public ClientManagementWindow(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            this.Loaded += ClientManagementWindow_Loaded;

            // Liaison initiale
            dgvClients.ItemsSource = _clients;
        }

        private async void ClientManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadClientsAsync();
        }

        // =======================================================
        // LECTURE (READ)
        // =======================================================
        private async Task LoadClientsAsync()
        {
            try
            {
                var clientsFromDb = await _unitOfWork.Clients.GetAllAsync();

                _clients.Clear();
                foreach (var client in clientsFromDb)
                {
                    _clients.Add(client);
                }
            }
            catch (Exception ex)
            {
                // Utilisation explicite de System.Windows.MessageBox pour éviter l'ambiguïté
                System.Windows.MessageBox.Show($"Erreur lors du chargement des clients: {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =======================================================
        // CRÉATION (CREATE)
        // =======================================================
        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            if (_selectedClient != null)
            {
                System.Windows.MessageBox.Show("Veuillez effacer le formulaire avant d'ajouter (Bouton 'Nouveau').", "Opération Invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dpBirthDate.SelectedDate.HasValue)
            {
                System.Windows.MessageBox.Show("Veuillez sélectionner une date de naissance valide.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newClient = new Client
            {
                ClientId = Guid.NewGuid(),
                FirstName = txtFirstName.Text,
                LastName = txtLastName.Text,
                Email = txtEmail.Text,
                Phone = txtPhone.Text,
                Address = txtAddress.Text,
                DriverLicenseNumber = txtDriverLicenseNumber.Text,
                BirthDate = dpBirthDate.SelectedDate.Value,
                CreatedAt = DateTime.UtcNow,
                Reservations = new List<Reservation>()
            };

            await _unitOfWork.Clients.AddAsync(newClient);
            await _unitOfWork.CompleteAsync();

            System.Windows.MessageBox.Show("Client ajouté avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadClientsAsync();
            ClearForm();
        }

        // =======================================================
        // MISE À JOUR (UPDATE)
        // =======================================================
        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null)
            {
                System.Windows.MessageBox.Show("Veuillez sélectionner un client à modifier.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateInput()) return;

            if (!dpBirthDate.SelectedDate.HasValue)
            {
                System.Windows.MessageBox.Show("Veuillez sélectionner une date de naissance valide.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _selectedClient.FirstName = txtFirstName.Text;
            _selectedClient.LastName = txtLastName.Text;
            _selectedClient.Email = txtEmail.Text;
            _selectedClient.Phone = txtPhone.Text;
            _selectedClient.Address = txtAddress.Text;
            _selectedClient.DriverLicenseNumber = txtDriverLicenseNumber.Text;
            _selectedClient.BirthDate = dpBirthDate.SelectedDate.Value;

            _unitOfWork.Clients.Update(_selectedClient);
            await _unitOfWork.CompleteAsync();

            System.Windows.MessageBox.Show("Client mis à jour avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadClientsAsync();
            ClearForm();
        }

        // =======================================================
        // SUPPRESSION (DELETE)
        // =======================================================
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null)
            {
                System.Windows.MessageBox.Show("Veuillez sélectionner un client à supprimer.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = System.Windows.MessageBox.Show($"Êtes-vous sûr de vouloir supprimer le client '{_selectedClient.FirstName} {_selectedClient.LastName}' ?\nCeci échouera s'il a des réservations actives.", "Confirmer Suppression", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _unitOfWork.Clients.DeleteAsync(_selectedClient.ClientId);
                    await _unitOfWork.CompleteAsync();

                    System.Windows.MessageBox.Show("Client supprimé avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadClientsAsync();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Erreur de suppression. Assurez-vous qu'aucune réservation n'est liée à ce client.\n{ex.Message}", "Erreur de Suppression", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // =======================================================
        // INTERFACE & VALIDATION
        // =======================================================

        private void DgvClients_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Reservations") e.Column.Visibility = Visibility.Collapsed;

            if (e.PropertyName == "ClientId") e.Column.Header = "ID Client";
            if (e.PropertyName == "FirstName") e.Column.Header = "Prénom";
            if (e.PropertyName == "LastName") e.Column.Header = "Nom";
            if (e.PropertyName == "DriverLicenseNumber") e.Column.Header = "Permis";
            if (e.PropertyName == "BirthDate")
            {
                e.Column.Header = "Date de Naissance";
                (e.Column as DataGridTextColumn)!.Binding.StringFormat = "dd/MM/yyyy";
            }
        }

        private void DgvClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvClients.SelectedItem is Client client)
            {
                _selectedClient = client;
                txtFirstName.Text = client.FirstName;
                txtLastName.Text = client.LastName;
                txtEmail.Text = client.Email;
                txtPhone.Text = client.Phone;
                txtAddress.Text = client.Address;
                txtDriverLicenseNumber.Text = client.DriverLicenseNumber;
                dpBirthDate.SelectedDate = client.BirthDate;
            }
            else
            {
                _selectedClient = null;
                // Pas de ClearForm(false) ici pour éviter les boucles, juste vider les champs si nécessaire ou laisser tel quel
            }
        }

        private void ClearForm(bool clearSelection = true)
        {
            txtFirstName.Text = string.Empty;
            txtLastName.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtPhone.Text = string.Empty;
            txtAddress.Text = string.Empty;
            txtDriverLicenseNumber.Text = string.Empty;
            dpBirthDate.SelectedDate = null;

            if (clearSelection)
            {
                _selectedClient = null;
                dgvClients.SelectedItem = null;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm(true);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtDriverLicenseNumber.Text) ||
                !dpBirthDate.SelectedDate.HasValue)
            {
                System.Windows.MessageBox.Show("Le Prénom, Nom, Email, Permis et Date de Naissance sont requis.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
    }
}