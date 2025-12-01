// Dans CarRental.Api.Repositories/TariffRepository.cs

using CarRental.Api.Data;
using CarRental2.Core.Entities;
using CarRental2.Core.Interfaces;

namespace CarRental.Api.Repositories
{
    public class TariffRepository : GenericRepository<Tariff>, ITariffRepository
    {
        private readonly ApplicationDbContext _context;

        public TariffRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // Si des méthodes spécifiques étaient ajoutées à ITariffRepository, 
        // elles seraient implémentées ici.
    }
}
