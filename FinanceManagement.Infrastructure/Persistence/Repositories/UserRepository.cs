using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            User user = await _db.Users.FindAsync(id);
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _db.Users.AnyAsync(user => user.Email == email);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _db.Users.SingleOrDefaultAsync(user => user.Email == email);
        }

        public async Task AddUserDataAsync(User user)
        {
            await _db.Users.AddAsync(user);
        }
    }
}
