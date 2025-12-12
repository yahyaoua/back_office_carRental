using CarRental2.Core.Interfaces.Services;
using CarRental2.Core.DTOs;
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

      
        public ICommand LoadSummaryCommand { get; private set; }
        public ICommand GeneratePdfCommand { get; private set; }

        public FinancialReportViewModel(IFinancialReportService reportService)
        {
            _reportService = reportService;

            // Initialisation des commandes (Utilise AsyncRelayCommand et RelayCommand)
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

        private FinancialSummaryDto _summary = new FinancialSummaryDto();
        public FinancialSummaryDto Summary
        {
            get => _summary;
            set { if (_summary != value) { _summary = value; OnPropertyChanged(); } }
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

                Summary = await _reportService.GetMonthlySummaryAsync(StartDate, EndDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du rapport : {ex.Message}", "Erreur Rapport DB", MessageBoxButton.OK, MessageBoxImage.Error);
                Summary = new FinancialSummaryDto();
            }
        }

        private void GeneratePdfReport()
        {
            MessageBox.Show("L'exportation PDF est en attente d'implémentation.", "Fonctionnalité PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }

   

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void GeneratePdfReport(object parameter) 
        {
            
            if (Summary.TotalReservationsCount == 0)
            {
                MessageBox.Show("Rapport vide. Veuillez charger les données d'abord.", "Alerte", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            MessageBox.Show("L'exportation PDF est en attente d'implémentation.", "Fonctionnalité PDF", MessageBoxButton.OK, MessageBoxImage.Information);

            
        }
    }
}