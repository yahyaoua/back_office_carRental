using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using CarRental.Api.Data;
using CarRental.Api.Repositories;
using CarRental.Api.Services;
using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace CarRental.Desktop.WPF
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=CarRental2Db_Dev;Trusted_Connection=True;MultipleActiveResultSets=true";

            // Configuration de la Base de Données
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITariffRepository, TariffRepository>();
            services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IFinancialReportService, FinancialReportService>();

            // Services Métier (BLL)
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IAuthService, AuthService>();

            // ENREGISTREMENT DU SERVICE DE STATISTIQUES (NOUVEAU)
            services.AddScoped<IStatsService, StatsService>();

            // Vues WPF (Fenêtres)
            services.AddTransient<MainWindow>();
            services.AddTransient<VehicleTypeManagementWindow>();
            services.AddTransient<ClientManagementWindow>();
            services.AddTransient<ReservationCreationWindow>();
            services.AddTransient<PaymentManagementWindow>();
            services.AddTransient<VehicleManagementWindow>();
            services.AddTransient<FinancialReportsWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                await _host.StartAsync();

                // Application des Migrations
                using (var scope = _host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    await context.Database.MigrateAsync();
                }

                // Affichage de la fenêtre principale
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur critique est survenue au démarrage : {ex.Message}", "Erreur de démarrage", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }
            base.OnExit(e);
        }
    }
}