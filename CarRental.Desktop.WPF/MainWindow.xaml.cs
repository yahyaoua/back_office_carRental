using CarRental2.Core.Interfaces;
using System.Windows;

// Importez les namespaces des fenêtres de gestion/création.
// Je suppose ici qu'elles sont toutes dans le namespace 'CarRental.Desktop.WPF'
// ou qu'elles sont rendues accessibles si elles sont dans des sous-dossiers.

namespace CarRental.Desktop.WPF
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Champ privé pour stocker l'unité de travail injectée
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructeur de MainWindow.
        /// </summary>
        /// <param name="unitOfWork">L'unité de travail pour les opérations de base de données.</param>
        public MainWindow(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
        }

        // =======================================================
        // GESTION DES OPÉRATIONS QUOTIDIENNES
        // =======================================================

        /// <summary>
        /// Ouvre la fenêtre de création d'une nouvelle réservation.
        /// </summary>
        private void BtnOpenReservationCreation_Click(object sender, RoutedEventArgs e)
        {
            var window = new ReservationCreationWindow(_unitOfWork);
            window.ShowDialog();
        }

        /// <summary>
        /// Ouvre la fenêtre de gestion et de modification des réservations existantes.
        /// </summary>
        //private void BtnOpenReservationManagement_Click(object sender, RoutedEventArgs e)
        //{
            //var window = new ReservationManagementWindow(_unitOfWork);
            //window.ShowDialog();
       // }

        /// <summary>
        /// Ouvre la fenêtre d'enregistrement et de consultation des paiements.
        /// </summary>
        private void BtnOpenPaymentManagement_Click(object sender, RoutedEventArgs e)
        {
            var window = new PaymentManagementWindow(_unitOfWork);
            window.ShowDialog();
        }

        // NOTE: Les gestionnaires pour "Gérer les Locations" et "Retour de Véhicule" ne sont pas encore implémentés
        // et sont souvent combinés ou gérés dans la ReservationManagementWindow.

        // =======================================================
        // GESTION DE L'INVENTAIRE ET DE LA CONFIGURATION
        // =======================================================

        /// <summary>
        /// Ouvre la fenêtre de gestion des types de véhicules.
        /// </summary>
        private void BtnVehicleTypeManagement_Click(object sender, RoutedEventArgs e)
        {
            var window = new VehicleTypeManagementWindow(_unitOfWork);
            window.ShowDialog();
        }

        /// <summary>
        /// Ouvre la fenêtre de gestion des tarifs.
        /// </summary>
        private void BtnTariffManagement_Click(object sender, RoutedEventArgs e)
        {
            var window = new TariffManagementWindow(_unitOfWork);
            window.ShowDialog();
        }

        /// <summary>
        /// Ouvre la fenêtre de gestion des véhicules (inventaire).
        /// </summary>
        private void BtnVehicleManagement_Click(object sender, RoutedEventArgs e)
        {
            var window = new VehicleManagementWindow(_unitOfWork);
            window.ShowDialog();
        }

        /// <summary>
        /// Ouvre la fenêtre de gestion des clients.
        /// </summary>
        private void BtnCustomerManagement_Click(object sender, RoutedEventArgs e)
        {
            // Correction : Utilisation de ClientManagementWindow comme demandé.
            var window = new ClientManagementWindow(_unitOfWork);
            window.ShowDialog();
        }
    }

    // ===================================================================================
    // ATTENTION : LES CLASSES DE FENÊTRE SUIVANTES DOIVENT EXISTER
    // (Ajoutées ici en commentaires pour rappel du contexte)
    // ===================================================================================

    /*
    public partial class ReservationCreationWindow : Window { public ReservationCreationWindow(IUnitOfWork uow) { } }
    public partial class ReservationManagementWindow : Window { public ReservationManagementWindow(IUnitOfWork uow) { } }
    public partial class PaymentManagementWindow : Window { public PaymentManagementWindow(IUnitOfWork uow) { } }
    public partial class VehicleTypeManagementWindow : Window { public VehicleTypeManagementWindow(IUnitOfWork uow) { } }
    public partial class TariffManagementWindow : Window { public TariffManagementWindow(IUnitOfWork uow) { } }
    public partial class VehicleManagementWindow : Window { public VehicleManagementWindow(IUnitOfWork uow) { } }
    public partial class ClientManagementWindow : Window { public ClientManagementWindow(IUnitOfWork uow) { } } // Mise à jour du nom de classe
    */
}