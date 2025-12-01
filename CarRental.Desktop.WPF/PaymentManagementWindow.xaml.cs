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

namespace CarRental.Desktop.WPF
{
    public partial class PaymentManagementWindow : Window
    {
        private readonly IUnitOfWork _unitOfWork;

        // Collections pour les DataGrids et ComboBox
        public ObservableCollection<Reservation> AvailableReservations { get; set; } = new ObservableCollection<Reservation>();
        public ObservableCollection<Payment> Payments { get; set; } = new ObservableCollection<Payment>();

        // Méthodes de paiement statiques (devraient idéalement être en Enum ou constantes)
        private static readonly List<string> PaymentMethods = new List<string> { "Card", "Cash", "Transfer", "Deposit Refund" };

        public PaymentManagementWindow(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            this.DataContext = this;
            this.Loaded += PaymentManagementWindow_Loaded;

            // Initialisation des méthodes de paiement
            cmbPaymentMethod.ItemsSource = PaymentMethods;
            if (PaymentMethods.Any())
            {
                cmbPaymentMethod.SelectedIndex = 0;
            }
        }

        private async void PaymentManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        // =======================================================
        // LECTURE (READ)
        // =======================================================

        /// <summary>
        /// Charge les réservations (pour lier les paiements) et tous les paiements.
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                // 1. Chargement des Réservations (pour le ComboBox)
                var reservations = await _unitOfWork.Reservations.GetAllAsync();
                AvailableReservations.Clear();
                // On inclut seulement les réservations qui ne sont pas annulées/terminées, ou toutes pour l'historique
                foreach (var res in reservations.Where(r => r.Status != "Cancelled" && r.Status != "NoShow").OrderBy(r => r.RequestedStart))
                {
                    AvailableReservations.Add(res);
                }

                // 2. Chargement de tous les Paiements
                var allPayments = await _unitOfWork.Payments.GetAllAsync();
                Payments.Clear();
                foreach (var payment in allPayments.OrderByDescending(p => p.PaymentDate))
                {
                    Payments.Add(payment);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données de configuration : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =======================================================
        // CRÉATION (CREATE) : Enregistrer un Nouveau Paiement
        // =======================================================
        private async void BtnRecordPayment_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput(out Reservation selectedReservation, out decimal amount, out string method)) return;

            try
            {
                var newPayment = new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    ReservationId = selectedReservation.ReservationId,
                    Amount = amount,
                    PaymentDate = DateTime.UtcNow, // Utilisation de UTC comme dans l'entité
                    PaymentMethod = method,
                    Status = "Completed" // Paiement enregistré manuellement est supposé réussi
                };

                await _unitOfWork.Payments.AddAsync(newPayment);
                await _unitOfWork.CompleteAsync();

                MessageBox.Show($"Paiement de {amount:C} enregistré avec succès pour la Réservation {selectedReservation.ReservationId}.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadDataAsync(); // Recharger les paiements
                ClearForm();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement du paiement : {ex.Message}", "Erreur DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Valide les champs de saisie du formulaire de paiement.
        /// </summary>
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

            if (!decimal.TryParse(txtAmount.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out amount) || amount == 0)
            {
                MessageBox.Show("Veuillez entrer un Montant valide (doit être non nul).", "Erreur de Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(method))
            {
                MessageBox.Show("Veuillez sélectionner une Méthode de Paiement.", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            cmbReservation.SelectedItem = null;
            txtAmount.Text = string.Empty;
            cmbPaymentMethod.SelectedIndex = 0; // Remettre par défaut
        }

        // =======================================================
        // UTILITAIRES : Affichage des montants dus/payés (optionnel)
        // =======================================================

        private void CmbReservation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbReservation.SelectedItem is Reservation selectedReservation)
            {
                decimal totalPaid = Payments
                    .Where(p => p.ReservationId == selectedReservation.ReservationId && p.Status == "Completed")
                    .Sum(p => p.Amount);

                decimal totalDue = selectedReservation.TotalAmount - totalPaid;

                // Affichage dans les TextBlocks
                txtTotalAmountDue.Text = selectedReservation.TotalAmount.ToString("F2", CultureInfo.CurrentCulture) + " €";
                txtTotalPaid.Text = totalPaid.ToString("F2", CultureInfo.CurrentCulture) + " €";
                txtBalance.Text = totalDue.ToString("F2", CultureInfo.CurrentCulture) + " €";

                // Suggestion de montant si le solde est positif
                if (totalDue > 0)
                {
                    txtAmount.Text = totalDue.ToString("F2", CultureInfo.CurrentCulture);
                }
                else if (totalDue < 0)
                {
                    // Suggestion de remboursement pour la caution par exemple
                    txtAmount.Text = Math.Abs(totalDue).ToString("F2", CultureInfo.CurrentCulture);
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
                txtAmount.Text = string.Empty;
            }
        }
    }
}