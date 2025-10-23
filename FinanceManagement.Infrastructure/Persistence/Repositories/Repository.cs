using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private ApplicationDbContext _db;
        internal DbSet<T> dbset;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            dbset = this._db.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllPopulatedAsync(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = dbset;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (include != null)
            {
                query = include(query);
            }
            return await query.ToListAsync();
        }

        public async Task<bool> TryAddAsync(T t)
        {
            try
            {
                await _db.AddAsync(t);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task AddAsync(T t)
        {
            await _db.AddAsync(t);
        }
        public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await dbset.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await dbset.Where(predicate).ToListAsync();
        }
        public void Update(T t)
        {
            _db.Update(t);
        }
        public void Delete(T t)
        {
            _db.Remove(t);
        }
    }
}
