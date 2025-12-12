using CarRental2.Core.Interfaces.Services;
using CarRental.Desktop.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CarRental.Desktop.WPF
{
    // Doit être public partial class FinancialReportsWindow
    public partial class FinancialReportsWindow : Window
    {
        // Le constructeur reçoit le service de DI
        public FinancialReportsWindow(IFinancialReportService financialReportService)
        {
            InitializeComponent();

            // 1. Instancier le ViewModel
            var viewModel = new FinancialReportViewModel(financialReportService);

            // 2. Définir le DataContext
            this.DataContext = viewModel;

            // 3. Charger les données au moment de l'affichage
            // Utilisation d'un lambda pour passer le ViewModel au gestionnaire d'événements
            this.Loaded += (sender, e) => FinancialReportsWindow_Loaded(viewModel);
        }

        private async void FinancialReportsWindow_Loaded(FinancialReportViewModel viewModel)
        {
            // Lancer la méthode de chargement du ViewModel au démarrage
            await viewModel.LoadSummaryAsync();
        }
    }
}