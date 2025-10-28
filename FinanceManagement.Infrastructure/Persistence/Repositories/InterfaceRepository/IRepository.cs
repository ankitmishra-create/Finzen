using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllPopulatedAsync(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);
        Task AddAsync(T entity);
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
        void Update(T entity);
        void Delete(T entity);
        Task<T> GetPopulatedAsync(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);
    }
}
