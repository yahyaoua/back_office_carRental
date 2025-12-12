using CarRental.Desktop.WPF.ViewModels;
using CarRental2.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarRental.Desktop.WPF
{
    /// <summary>
    /// Logique d'interaction pour DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : UserControl
    {
        private readonly DashboardViewModel _viewModel;

        // Le IStatsService est injecté dans le constructeur
        public DashboardPage(IStatsService statsService)
        {
            InitializeComponent();

            // 1. Créer le ViewModel en lui injectant le service
            _viewModel = new DashboardViewModel(statsService);

            // 2. Lier le ViewModel à l'interface (UserControl)
            this.DataContext = _viewModel;

            // 3. Charger les stats lorsque le contrôle est prêt
            // L'événement 'Loaded' est le bon endroit pour déclencher le chargement asynchrone
            this.Loaded += DashboardPage_Loaded;
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Appel de la méthode de chargement du ViewModel
            await _viewModel.LoadStatsAsync();
        }
    }
}
