// Fichier: CarRental.Api/Repositories/GenericRepository.cs

using CarRental2.Core.Interfaces;
using CarRental.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
// Ajouté pour supporter les méthodes .Include() dans GetAllAsync
using Microsoft.EntityFrameworkCore.Query;

namespace CarRental.Api.Repositories
{
    // Assurez-vous que l'interface IGenericRepository<T> a été mise à jour dans le projet CarRental2.Core.
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        // --- READ ---

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        // CORRECTION FINALE : Implémentation de GetAllAsync pour supporter le filtre et le Eager Loading (Include).
        public async Task<IReadOnlyList<T>> GetAllAsync(
            Expression<Func<T, bool>> filter = null,
            // Utilisez Func<IQueryable<T>, IQueryable<T>> pour être compatible avec l'interface Core,
            // même si l'implémentation EF Core utilise IIncludableQueryable en interne.
            Func<IQueryable<T>, IQueryable<T>> include = null)
        {
            IQueryable<T> query = _dbSet;

            // 1. Applique le Eager Loading
            if (include != null)
            {
                // Appelle la fonction lambda qui contient les .Include(s)
                query = include(query);
            }

            // 2. Applique le filtre
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // 3. Exécute la requête
            return await query.ToListAsync();
        }

        // --- CREATE ---

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        // --- UPDATE ---

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        // --- DELETE ---

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);

            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }
    }
}