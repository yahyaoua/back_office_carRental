// Dans CarRental.Api.Repositories/VehicleTypeRepository.cs

using CarRental.Api.Data;
using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces;

namespace CarRental.Api.Repositories
{
    public class VehicleTypeRepository : GenericRepository<VehicleType>, IVehicleTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleTypeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // Les méthodes du CRUD sont héritées de GenericRepository.
    }
}