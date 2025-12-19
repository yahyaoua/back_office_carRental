using CarRental2.Core.Interfaces.Services;
using CarRental2.Core.DTOs;
using CarRental.Api.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace CarRental.Desktop.WPF.ViewModels
{
    public class FinancialReportViewModel : INotifyPropertyChanged
    {
        private readonly IFinancialReportService _reportService;

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand LoadSummaryCommand { get; private set; }
        public ICommand GeneratePdfCommand { get; private set; }

        public FinancialReportViewModel(IFinancialReportService reportService)
        {
            _reportService = reportService;

            // Initialisation des commandes
            LoadSummaryCommand = new AsyncRelayCommand(async (parameter) => await LoadSummaryAsync());
            GeneratePdfCommand = new RelayCommand(GeneratePdfReport);

            // Dates par défaut
            _startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _endDate = DateTime.Today;
        }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set { if (_startDate != value) { _startDate = value; OnPropertyChanged(); } }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set { if (_endDate != value) { _endDate = value; OnPropertyChanged(); } }
        }

        // CHANGEMENT ICI : Renommé de 'Summary' à 'Report' pour matcher le XAML Pro
        private FinancialReportDto _report = new FinancialReportDto();
        public FinancialReportDto Report
        {
            get => _report;
            set { if (_report != value) { _report = value; OnPropertyChanged(); } }
        }

        public async Task LoadSummaryAsync()
        {
            try
            {
                if (StartDate > EndDate)
                {
                    MessageBox.Show("La date de début ne peut pas être postérieure à la date de fin.", "Erreur de Période", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Affectation à 'Report' pour déclencher la mise à jour de la DataGrid
                Report = await _reportService.GetFinancialReportAsync(StartDate, EndDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du rapport : {ex.Message}", "Erreur Rapport DB", MessageBoxButton.OK, MessageBoxImage.Error);
                Report = new FinancialReportDto();
            }
        }

        private void GeneratePdfReport(object parameter)
        {
            if (Report == null || Report.TotalReservationsCount == 0)
            {
                MessageBox.Show("Veuillez d'abord charger des données.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Fichier PDF (*.pdf)|*.pdf",
                FileName = $"Rapport_Financier_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var pdfService = new PdfReportService();
                    pdfService.ExportFinancialReport(Report, saveFileDialog.FileName);

                    var result = MessageBox.Show("Rapport généré avec succès ! Voulez-vous l'ouvrir ?", "Succès", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la génération du PDF : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}