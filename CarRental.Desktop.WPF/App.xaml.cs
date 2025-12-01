using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using CarRental.Api.Data; // Pour ApplicationDbContext
using CarRental.Api.Repositories; // Pour GenericRepository, UnitOfWork, etc.
using CarRental.Api.Services; // Pour AuthService, VehicleService, etc.
using CarRental2.Core.Interfaces; // Pour IUnitOfWork, IGenericRepository, etc.
using CarRental2.Core.Interfaces.Services; // Pour IAuthService, IVehicleService, etc.
using System;
using System.Threading.Tasks;

namespace CarRental.Desktop.WPF
{
    // Cette classe configure l'hôte générique, la base de données et l'injection de dépendances
    // pour l'ensemble de l'application WPF.
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            // 1. Initialisation de l'hôte (Host Builder)
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // NOTE: Récupérez cette chaîne de connexion de votre appsettings.json ou d'une source fiable.
            // La chaîne est laissée ici comme exemple.
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=CarRental2Db_Dev;Trusted_Connection=True;MultipleActiveResultSets=true";

            // --- 2. CONFIGURATION DE LA BASE DE DONNÉES (DAL) ---
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // --- 3. ENREGISTREMENT DES COUCHES DAL et BLL (Vos Repositories et Services) ---

            // Repositories génériques et spécifiques
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITariffRepository, TariffRepository>();
            services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();

            // L'Unit of Work (qui contient tous les Repositories)
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Couche de Services (BLL)
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IAuthService, AuthService>();

            // --- 4. ENREGISTREMENT DES VUES WPF (Fenêtres) ---
            // Toutes les fenêtres utilisées doivent être enregistrées ici comme Transient.
            services.AddTransient<MainWindow>();
            services.AddTransient<TariffManagementWindow>();
            services.AddTransient<VehicleTypeManagementWindow>();
            services.AddTransient<ClientManagementWindow>();
            services.AddTransient<ReservationCreationWindow>();
            services.AddTransient<PaymentManagementWindow>();
            services.AddTransient<VehicleManagementWindow>();
            
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Démarre l'hôte
                await _host.StartAsync();

                // 5. Initialisation de la base de données
                using (var scope = _host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    await context.Database.MigrateAsync();
                }

                // 6. Récupère la fenêtre principale via la DI et l'affiche
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