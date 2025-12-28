using CarRental.Api.Services;
using CarRental.Desktop.WPF.ViewModels;
using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Drawing; 
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
using WinForms = System.Windows.Forms;

namespace CarRental.Desktop.WPF
{
    
    public partial class DashboardPage : UserControl
    {
        private readonly DashboardViewModel _viewModel;

        private readonly IVehicleService _vehicleService;

        
        public DashboardPage(IStatsService statsService, IVehicleService vehicleService)
        {
            InitializeComponent();

            // 1. Créer le ViewModel en lui injectant le service
            _viewModel = new DashboardViewModel(statsService);

            
            this.DataContext = _viewModel;

            _vehicleService = vehicleService;

            this.Loaded += DashboardPage_Loaded;
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            
            await _viewModel.LoadStatsAsync();
            await CheckAndNotifyUpcomingMaintenance(3);
        }

        private async Task CheckAndNotifyUpcomingMaintenance(int daysThreshold)
        {
            try
            {
                
                var upcoming = await _vehicleService.GetUpcomingMaintenancesAsync(daysThreshold);

                if (upcoming != null && upcoming.Any())
                {
                    int count = upcoming.Count();
                    string title = "⚠️ Rappel Maintenance";
                    string message;

                    if (count == 1)
                    {
                        var m = upcoming.First();
                        
                        string plaque = m.Vehicle?.PlateNumber ?? "Inconnu";
                        message = $"Entretien '{m.Type}' prévu le {m.ScheduledDate:dd/MM} pour {plaque}.";
                    }
                    else
                    {
                        message = $"{count} véhicules nécessitent une maintenance dans les {daysThreshold} jours !";
                    }

                    ShowWindowsNotification(title, message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur notif : {ex.Message}");
            }
        }

        private void ShowWindowsNotification(string title, string message)
        {
            
            using (var notifyIcon = new WinForms.NotifyIcon())
            {
                notifyIcon.Icon = SystemIcons.Warning; 
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(5000, title, message, WinForms.ToolTipIcon.Warning);
            }
        }
    
}
}
