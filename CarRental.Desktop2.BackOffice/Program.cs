// Dans CarRental.Desktop.BackOffice/Program.cs

using CarRental.Api.Data;
using CarRental.Api.Repositories;
using CarRental.Api.Services;
using CarRental.Desktop2.BackOffice;
using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Forms;

namespace CarRental.Desktop.BackOffice
{
    static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        [STAThread]
        static void Main()
        {
            // 1. Initialiser le conteneur de services
            ConfigureServices();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 2. Lancer la Forme principale (Doit exister dans votre projet)
            // Assurez-vous d'avoir une MainForm ou une Form principale nommée comme ceci.
            var mainForm = ServiceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // =========================================================
            // A. CONFIGURATION DE LA BASE DE DONNÉES (DAL)
            // =========================================================

            // Récupérez la chaîne de connexion de votre appsettings.json
            // REMPLACER PAR VOTRE CHAÎNE DE CONNEXION VALIDE POUR (localdb)
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=CarRental2Db_Dev;Trusted_Connection=True;MultipleActiveResultSets=true";

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // =========================================================
            // B. ENREGISTREMENT DES COUCHES DAL et BLL
            // =========================================================

            // Enregistrement de la DAL (Repositories)
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Repositories spécifiques (selon votre IUnitOfWork final)
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITariffRepository, TariffRepository>(); // CORRIGÉ
            services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>(); // CORRIGÉ

            // L'Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Enregistrement de la BLL (Services Métier)
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IVehicleService, VehicleService>();

            // =========================================================
            // C. ENREGISTREMENT DES FORMES WINFORMS
            // =========================================================

            // Enregistrez toutes les formes que vous prévoyez d'utiliser
            services.AddTransient<MainForm>();
            services.AddTransient<TariffManagementForm>();
            // Ajoutez ici les autres formes comme ClientManagementForm, TariffManagementForm, etc.

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}