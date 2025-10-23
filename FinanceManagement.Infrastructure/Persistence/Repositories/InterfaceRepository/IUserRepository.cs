using FinanceManagement.Core.Entities;

namespace FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByIdAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);
        Task AddUserAsync(User user);
        Task<User> GetByEmailAsync(string email);

    }
}
