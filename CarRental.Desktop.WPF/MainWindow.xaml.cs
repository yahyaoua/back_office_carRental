using CarRental.Api.Services;
using CarRental.Desktop.WPF;
using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using System; // Ajouté pour Exception
using System.Windows;

namespace CarRental.Desktop.WPF
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVehicleService _vehicleService;
        private readonly IStatsService _statsService;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Constructeur de MainWindow avec injection de dépendances.
        /// </summary>
        public MainWindow(IUnitOfWork unitOfWork, IVehicleService vehicleService, IStatsService statsService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _vehicleService = vehicleService; 
            _statsService = statsService;
            _serviceProvider = serviceProvider;

            InitializeViews();
        }

        private void InitializeViews()
        {
            var dashboardControl = new DashboardPage(_statsService, _vehicleService);

            if (MainDashboardContent != null)
            {
                MainDashboardContent.Content = dashboardControl;
            }
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
        // GESTION DE LA MAINTENANCE
        // =======================================================

        private void BtnOpenMaintenanceManagement_Click(object sender, RoutedEventArgs e)
        {
            
            var window = new MaintenanceManagementWindow(_vehicleService);
            window.Show();
        }

        private void BtnOpenFinancialReports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var reportsWindow = _serviceProvider.GetRequiredService<FinancialReportsWindow>();
                reportsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture du rapport : {ex.Message}", "Erreur DI", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}