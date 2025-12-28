using CarRental.Api.Data;
using CarRental.Api.Repositories;
using CarRental.Api.Services;
using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

// 🔐 AJOUT AUTH COOKIE
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// HTTP CLIENTS (SSL DEV LOCAL - IMPORTANT)
// =========================================================

// Client nommé (si utilisé ailleurs)
builder.Services.AddHttpClient("ApiClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

// Client par défaut (UTILISÉ PAR AccountController)
builder.Services.AddHttpClient("DefaultClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

// =========================================================
// SESSION (EXISTANTE - ON GARDE)
// =========================================================
builder.Services.AddSession();

// =========================================================
// 🔐 AUTHENTIFICATION COOKIE (AJOUT)
// =========================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// =========================================================
// 1. CONFIGURATION DE LA BASE DE DONNÉES (DAL)
// =========================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La chaîne de connexion 'DefaultConnection' n'a pas été trouvée.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// =========================================================
// 2. ENREGISTREMENT DES COUCHES DAL et BLL
// =========================================================

// --- 2.1 DAL ---
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITariffRepository, TariffRepository>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- 2.2 BLL ---
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// =========================================================
// 3. MVC + API
// =========================================================
builder.Services.AddControllersWithViews();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =========================================================
// 4. PIPELINE HTTP
// =========================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// =========================================================
// SESSION + AUTH (ORDRE CRUCIAL)
// =========================================================
app.UseSession();

app.UseAuthentication();   // 🔐 AJOUT (OBLIGATOIRE)
app.UseAuthorization();

// =========================================================
// 5. ROUTES MVC + API
// =========================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// API Controllers (/api/...)
app.MapControllers();

// =========================================================
// 6. SEED DATABASE
// =========================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.Run();
