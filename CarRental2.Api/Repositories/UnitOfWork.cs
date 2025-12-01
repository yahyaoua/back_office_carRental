// Dans CarRental.Api/Repositories/UnitOfWork.cs (VERSION FINALE)

using CarRental2.Core.Interfaces;
using CarRental.Api.Data;
using CarRental.Api.Services;
using CarRental2.Core.Entities;

namespace CarRental.Api.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        // Repositories spécifiques (champs privés avec '!' pour éviter les warnings)
        private IVehicleRepository _vehicles = null!;
        private IReservationRepository _reservations = null!;
        private IClientRepository _clients = null!;
        private IUserRepository _users = null!;

        private ITariffRepository _tariffs = null!;
        private IVehicleTypeRepository _vehicleTypes = null!; // <-- NOUVEAU

        // Repositories génériques (les entités plus simples)
        private IGenericRepository<Maintenance> _maintenances = null!;
        private IGenericRepository<Payment> _payments = null!;
        private IGenericRepository<VehicleImage> _vehicleImages = null!;


        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ====================================================
        // Propriétés publiques (Implémentation de l'IUnitOfWork)
        // ====================================================

        public IVehicleRepository Vehicles => _vehicles ??= new VehicleRepository(_dbContext);
        public IReservationRepository Reservations => _reservations ??= new ReservationRepository(_dbContext);
        public IClientRepository Clients => _clients ??= new ClientRepository(_dbContext);
        public IUserRepository Users => _users ??= new UserRepository(_dbContext);

        // Repositories de configuration (spécifiques)
        public ITariffRepository Tariffs => _tariffs ??= new TariffRepository(_dbContext);
        public IVehicleTypeRepository VehicleTypes => _vehicleTypes ??= new VehicleTypeRepository(_dbContext); // <-- NOUVEAU

        // Repositories génériques
        public IGenericRepository<Maintenance> Maintenances =>
            _maintenances ??= new GenericRepository<Maintenance>(_dbContext);
        public IGenericRepository<Payment> Payments =>
            _payments ??= new GenericRepository<Payment>(_dbContext);
        public IGenericRepository<VehicleImage> VehicleImages =>
            _vehicleImages ??= new GenericRepository<VehicleImage>(_dbContext);

        // ... (CompleteAsync et Dispose)

        public async Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}