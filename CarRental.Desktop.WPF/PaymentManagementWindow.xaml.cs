using CarRental2.Core.Interfaces;
using CarRental2.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Data;
using System.Threading; // IMPORTER CECI pour le SemaphoreSlim
using System.Windows.Controls.Primitives; // IMPORTER CECI pour Selector

namespace CarRental.Desktop.WPF
{

    // =======================================================
    // FENÊTRE PRINCIPALE
    // =======================================================
    public partial class PaymentManagementWindow : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        // PROTECTION CONTRE L'ACCÈS CONCURRENTIEL À LA DB
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        // --- COLLECTIONS ET ÉTATS ---
        public ObservableCollection<Client> AvailableClients { get; set; } = new ObservableCollection<Client>();
        private ObservableCollection<Reservation> AllReservations { get; set; } = new ObservableCollection<Reservation>();
        public ObservableCollection<Reservation> FilteredReservations { get; set; } = new ObservableCollection<Reservation>();

        public ObservableCollection<Payment> Payments { get; set; } = new ObservableCollection<Payment>();
        private Payment _selectedPayment;
        public List<string> PaymentMethodsList { get; } = new List<string> { "Card", "Cash", "Transfer", "Deposit Refund" };


        public PaymentManagementWindow(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            this.DataContext = this;
            this.Loaded += PaymentManagementWindow_Loaded;

            cmbPaymentMethod.ItemsSource = PaymentMethodsList;
            if (PaymentMethodsList.Any())
            {
                cmbPaymentMethod.SelectedIndex = 0;
            }
        }

        private void BtnClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        // PROTECTION CONTRE L'ACCÈS CONCURRENTIEL
        private async void PaymentManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
            ClearForm();
        }

        // =======================================================
        // LECTURE (READ) : Chargement initial des données
        // =======================================================

        private async Task LoadDataAsync()
        {
            await _lock.WaitAsync();
            try
            {
                // 1. Chargement des Clients
                var clients = await _unitOfWork.Clients.GetAllAsync();
                AvailableClients.Clear();
                foreach (var client in clients.OrderBy(c => c.LastName).ThenBy(c => c.FirstName))
                {
                    AvailableClients.Add(client);
                }

                // 2. Chargement de TOUTES les Réservations
                var reservations = (await _unitOfWork.Reservations.GetAllAsync())
                                    .Where(r => r.Status != "Cancelled" && r.Status != "NoShow")
                                    .OrderBy(r => r.RequestedStart);

                AllReservations.Clear();
                foreach (var res in reservations)
                {
                    AllReservations.Add(res);
                }

                // Initialiser les réservations visibles (toutes par défaut)
                UpdateFilteredReservations(null);

                // 3. Chargement de tous les Paiements
                var allPayments = (await _unitOfWork.Payments.GetAllAsync()).OrderByDescending(p => p.PaymentDate);
                Payments.Clear();
                foreach (var payment in allPayments)
                {
                    Payments.Add(payment);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Met à jour la liste des réservations affichées dans la ComboBox en fonction du Client.
        /// </summary>
        private void UpdateFilteredReservations(Guid? clientId)
        {
            FilteredReservations.Clear();
            var reservationsToShow = AllReservations.AsEnumerable();

            if (clientId.HasValue)
            {
                reservationsToShow = AllReservations.Where(r => r.ClientId == clientId.Value);
            }

            foreach (var res in reservationsToShow.OrderBy(r => r.RequestedStart))
            {
                FilteredReservations.Add(res);
            }
        }

        // =======================================================
        // FILTRAGE ET SÉLECTION
        // =======================================================

        /// <summary>
        /// Gère le changement de sélection du client pour filtrer les réservations.
        /// </summary>
        private void CmbClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedClient = cmbClient.SelectedItem as Client;
            Guid? clientId = selectedClient?.ClientId;

            // 1. Filtrer les réservations
            UpdateFilteredReservations(clientId);

            // 2. Réinitialiser la sélection de la réservation et les synthèses
            cmbReservation.SelectedItem = null;

            // 3. Réinitialiser les synthèses et la grille des paiements
            LoadPaymentsForFilteredReservations(); // Appel ASYNC
            UpdateSummary(null);
            ClearForm(false);
        }

        /// <summary>
        /// Charge les paiements liés aux réservations actuellement affichées (filtrées par client).
        /// PROTECTION CONTRE L'ACCÈS CONCURRENTIEL
        /// </summary>
        private async void LoadPaymentsForFilteredReservations()
        {
            await _lock.WaitAsync();
            try
            {
                var allPayments = await _unitOfWork.Payments.GetAllAsync();

                Payments.Clear();
                var selectedClient = cmbClient.SelectedItem as Client;

                var paymentsToShow = allPayments.AsEnumerable();

                if (selectedClient != null)
                {
                    var reservationIds = FilteredReservations.Select(r => r.ReservationId).ToList();
                    paymentsToShow = allPayments.Where(p => reservationIds.Contains(p.ReservationId));
                }

                foreach (var payment in paymentsToShow.OrderByDescending(p => p.PaymentDate))
                {
                    Payments.Add(payment);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du filtrage des paiements : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gère le changement de sélection de la réservation pour afficher la synthèse.
        /// </summary>
        private void CmbReservation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbReservation.SelectedItem is Reservation selectedReservation)
            {
                UpdateSummary(selectedReservation);
                FilterPaymentsByReservation(selectedReservation.ReservationId); // Appel ASYNC
            }
            else
            {
                UpdateSummary(null);
                LoadPaymentsForFilteredReservations();
            }
            ClearForm(false);
        }

        /// <summary>
        /// Filtre la liste des paiements affichés par une réservation spécifique.
        /// PROTECTION CONTRE L'ACCÈS CONCURRENTIEL
        /// </summary>
        private async void FilterPaymentsByReservation(Guid reservationId)
        {
            await _lock.WaitAsync();
            try
            {
                var allPayments = await _unitOfWork.Payments.GetAllAsync();
                Payments.Clear();

                foreach (var payment in allPayments.Where(p => p.ReservationId == reservationId).OrderByDescending(p => p.PaymentDate))
                {
                    Payments.Add(payment);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du filtrage des paiements : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _lock.Release();
            }
        }

        // RENDUE ASYNC ET PROTÉGÉE POUR LE CALCUL DB
        private async void UpdateSummary(Reservation selectedReservation)
        {
            await _lock.WaitAsync();
            try
            {
                if (selectedReservation != null)
                {
                    // Calculer le total payé pour la réservation sélectionnée (ASYNCHRONE)
                    decimal totalPaid = (await _unitOfWork.Payments.GetAllAsync())
                        .Where(p => p.ReservationId == selectedReservation.ReservationId && p.Status == "Completed")
                        .Sum(p => p.Amount);

                    decimal totalDue = selectedReservation.TotalAmount - totalPaid;

                    // Affichage dans les TextBlocks
                    txtTotalAmountDue.Text = selectedReservation.TotalAmount.ToString("F2", CultureInfo.CurrentCulture) + " €";
                    txtTotalPaid.Text = totalPaid.ToString("F2", CultureInfo.CurrentCulture) + " €";
                    txtBalance.Text = totalDue.ToString("F2", CultureInfo.CurrentCulture) + " €";

                    txtBalance.Foreground = totalDue >= 0 ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green;

                    // Suggestion de montant
                    if (totalDue > 0)
                    {
                        txtAmount.Text = totalDue.ToString("F2", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        txtAmount.Text = "0.00";
                    }
                }
                else
                {
                    txtTotalAmountDue.Text = "N/A";
                    txtTotalPaid.Text = "N/A";
                    txtBalance.Text = "N/A";
                    txtBalance.Foreground = System.Windows.Media.Brushes.Red;
                    txtAmount.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour de la synthèse : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _lock.Release();
            }
        }


        // DANS PaymentManagementWindow.xaml.cs

        private void DgvPayments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 1. Définir et vérifier la sélection immédiatement
            // Utiliser 'as Payment' et vérifier le null
            _selectedPayment = dgvPayments.SelectedItem as Payment;

            if (_selectedPayment != null)
            {
                // On est en mode MODIFICATION/SUPPRESSION

                // Trouver la Réservation correspondante en utilisant la collection complète AllReservations
                var correspondingReservation = AllReservations.FirstOrDefault(r => r.ReservationId == _selectedPayment.ReservationId);

                if (correspondingReservation != null)
                {
                    // 2. Sélectionner le Client (Ceci va déclencher CmbClient_SelectionChanged)
                    cmbClient.SelectedValue = correspondingReservation.ClientId;

                    
                    cmbReservation.SelectedValue = _selectedPayment.ReservationId; 

                    
                }
                else
                {
                    
                    ClearForm(true);
                }
            }
            else
            {
                
                ClearForm(false);
            }
        }

        private void ClearForm(bool clearClientAndReservation = true)
        {
            _selectedPayment = null;
            if (clearClientAndReservation)
            {
                cmbClient.SelectedItem = null;
            }

            dgvPayments.SelectedItem = null;
            txtAmount.Text = string.Empty;
            cmbPaymentMethod.SelectedIndex = 0;

            BtnRecordPayment.IsEnabled = true;
            
        }

        // =======================================================
        // CRUD (Create, Update, Delete)
        // =======================================================

        private async void BtnRecordPayment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPayment != null) return;
            if (!ValidateInput(out Reservation selectedReservation, out decimal amount, out string method)) return;

            await _lock.WaitAsync();
            try
            {
                var newPayment = new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    ReservationId = selectedReservation.ReservationId,
                    Amount = amount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = method,
                    Status = "Completed"
                };

                await _unitOfWork.Payments.AddAsync(newPayment);
                await _unitOfWork.CompleteAsync();

                MessageBox.Show($"Paiement de {amount:C} enregistré avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadDataAsync();
                UpdateSummary(selectedReservation);
                ClearForm(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement du paiement : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _lock.Release();
            }
        }

        

        

        // =======================================================
        // VALIDATION
        // =======================================================

        private bool ValidateInput(out Reservation selectedReservation, out decimal amount, out string method)
        {
            selectedReservation = cmbReservation.SelectedItem as Reservation;
            amount = 0;
            method = cmbPaymentMethod.SelectedItem as string;

            if (selectedReservation == null)
            {
                MessageBox.Show("Veuillez sélectionner une Réservation.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!decimal.TryParse(txtAmount.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out amount) || amount <= 0)
            {
                MessageBox.Show("Veuillez entrer un Montant valide (doit être non nul et positif).", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(method))
            {
                MessageBox.Show("Veuillez sélectionner une Méthode de Paiement.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool ValidateUpdateInput(out decimal amount, out string method)
        {
            amount = 0;
            method = cmbPaymentMethod.SelectedItem as string;

            if (!decimal.TryParse(txtAmount.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out amount) || amount <= 0)
            {
                MessageBox.Show("Veuillez entrer un Montant valide (doit être non nul et positif).", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(method))
            {
                MessageBox.Show("Veuillez sélectionner une Méthode de Paiement.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void DgvPayments_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Peut être laissé vide si toutes les colonnes sont définies en XAML
        }
    }
}