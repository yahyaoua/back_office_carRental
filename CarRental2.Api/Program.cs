using CarRental.Api.Data;
using CarRental.Api.Repositories;
using CarRental.Api.Services;
using CarRental2.Core.Interfaces;
using CarRental2.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- 2.2 BLL ---
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// =========================================================
// 3. MVC + API (IMPORTANT)
// =========================================================
// ? Remplace AddControllers() par AddControllersWithViews() pour supporter View(), TempData, Razor, etc.
builder.Services.AddControllersWithViews();

// Swagger (garde ta logique)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =========================================================
// 4. PIPELINE HTTP (ordre correct)
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

app.UseAuthorization();

// =========================================================
// 5. ROUTES (MVC + API) - UNE SEULE FOIS
// =========================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// ? Pour tes controllers API (avec [ApiController] et routes /api/...)
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbSeeder.SeedAsync(db);
}


app.Run();
