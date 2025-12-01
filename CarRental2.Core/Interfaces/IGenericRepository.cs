// Dans CarRental2.Core/Interfaces/IGenericRepository.cs

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CarRental2.Core.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // READ
        Task<T> GetByIdAsync(Guid id);
        Task<IReadOnlyList<T>> GetAllAsync();

        // READ avec filtre (optionnel mais très utile)
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // CREATE
        Task AddAsync(T entity);

        // UPDATE
        void Update(T entity);

        // DELETE
        void Delete(T entity);
        Task DeleteAsync(Guid id);
    }
}