// Dans CarRental.Api/Program.cs

using CarRental.Api.Data;
using CarRental.Api.Repositories;
using CarRental.Api.Services;
using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // Fournit AddScoped, AddTransient, IServiceCollection
using Microsoft.EntityFrameworkCore;             // Fournit l'extension UseSqlServer
using CarRental.Api.Data;                      // Pour ApplicationDbContext
using CarRental2.Core.Interfaces;              // Pour IUnitOfWork et IGenericRepository

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. CONFIGURATION DE LA BASE DE DONNÉES (DAL)
// =========================================================

// Récupération de la chaîne de connexion à partir du fichier de configuration (appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La chaîne de connexion 'DefaultConnection' n'a pas été trouvée.");
}

// Enregistrement de ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// =========================================================
// 2. ENREGISTREMENT DES COUCHES DAL et BLL
// =========================================================

// --- 2.1 Enregistrement de la Couche d'Accès aux Données (DAL) ---

// Repositories génériques et spécifiques
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<ITariffRepository, TariffRepository>();


// L'Unit of Work (qui utilise tous les Repositories injectés)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// --- 2.2 Enregistrement de la Couche de Services (BLL) ---

builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IAuthService, AuthService>();


// =========================================================
// 3. CONFIGURATION STANDARD DE L'API
// =========================================================

// Ajout du support des contrôleurs
builder.Services.AddControllers();

// Ajout de Swagger/OpenAPI pour la documentation des endpoints
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configuration du pipeline HTTP

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// app.UseAuthentication(); // À ajouter après la configuration JWT
app.UseAuthorization();

app.MapControllers();

app.Run();