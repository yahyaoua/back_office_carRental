using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using System.Windows;

namespace CarRental.Desktop.WPF
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        // Injecter le service de véhicules/maintenance spécifique
        private readonly IVehicleService _vehicleService;

        /// <summary>
        /// Constructeur de MainWindow avec injection de dépendances.
        /// </summary>
        public MainWindow(IUnitOfWork unitOfWork, IVehicleService vehicleService)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _vehicleService = vehicleService; // Initialisation
        }

        // =======================================================
        // GESTION DES OPÉRATIONS QUOTIDIENNES
        // =======================================================

        private void BtnOpenReservationCreation_Click(object sender, RoutedEventArgs e)
        {
            var window = new ReservationCreationWindow(_unitOfWork);
            window.ShowDialog();
        }

        private void BtnOpenPaymentManagement_Click(object sender, RoutedEventArgs e)
        {
            var window = new PaymentManagementWindow(_unitOfWork);
            window.ShowDialog();
        }

        // =======================================================
        // GESTION DE L'INVENTAIRE ET DE LA CONFIGURATION
        // =======================================================

        private void BtnVehicleTypeManagement_Click(object sender, RoutedEventArgs e)
        {
            var window = new VehicleTypeManagementWindow(_unitOfWork);
            window.ShowDialog();
        }

        private void BtnTariffManagement_Click(object sender, RoutedEventArgs e)
        {
            var window = new TariffManagementWindow(_unitOfWork);
            window.ShowDialog();
        }

        private void BtnVehicleManagement_Click(object sender, RoutedEventArgs e)
        {
            var window = new VehicleManagementWindow(_unitOfWork);
            window.ShowDialog();
        }

        private void BtnCustomerManagement_Click(object sender, RoutedEventArgs e)
        {
            var window = new ClientManagementWindow(_unitOfWork);
            window.ShowDialog();
        }

        // =======================================================
        // GESTION DE LA MAINTENANCE (CORRIGÉ)
        // =======================================================

        private void BtnOpenMaintenanceManagement_Click(object sender, RoutedEventArgs e)
        {
            // Correction : Passage de IVehicleService
            var window = new MaintenanceManagementWindow(_vehicleService);
            window.Show();
        }
    }
}