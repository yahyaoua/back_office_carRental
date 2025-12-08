// Dans CarRental2.Core/Interfaces/IGenericRepository.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public interface IGenericRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id);

    // CORRECTION : L'interface doit déclarer les deux paramètres optionnels.
    // Nous utilisons Func<IQueryable<T>, IQueryable<T>> car il est plus neutre pour la couche Core.
    Task<IReadOnlyList<T>> GetAllAsync(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IQueryable<T>> include = null);

    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task DeleteAsync(Guid id);
}