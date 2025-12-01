// Dans CarRental.Api/Repositories/GenericRepository.cs

using CarRental2.Core.Interfaces;
using CarRental.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarRental.Api.Repositories
{
    // T est le type d'entité (ex: Vehicle, Client)
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>(); // Obtient le DbSet correspondant à l'entité T
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // Implémentation du filtre FindAsync
        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            // NOTE: Nous n'appelons pas SaveChangesAsync ici. 
            // C'est le rôle de l'IUnitOfWork (CompleteAsync) de le faire.
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
            // NOTE: Le changement d'état est suivi par EF Core jusqu'à CompleteAsync.
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
        public async Task DeleteAsync(Guid id)
        {
            // 1. Trouver l'entité par son ID
            var entity = await _dbSet.FindAsync(id);

            // 2. Supprimer l'entité si elle existe
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
            // Note : Le CompleteAsync() est appelé dans IUnitOfWork.CompleteAsync() par le formulaire.
        }
    }
}