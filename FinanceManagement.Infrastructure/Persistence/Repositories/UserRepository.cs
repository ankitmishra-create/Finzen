using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            User user = await _db.Users.SingleOrDefaultAsync(user => user.UserId == id);
            return user ?? null;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (await _db.Users.SingleOrDefaultAsync(user => user.Email == email) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _db.Users.SingleOrDefaultAsync(user => user.Email == email);
        }

        public async Task AddUserAsync(User user)
        {
            await _db.AddAsync(user);
        }
    }
}
