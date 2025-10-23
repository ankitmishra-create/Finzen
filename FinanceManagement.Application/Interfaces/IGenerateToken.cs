using FinanceManagement.Core.Entities;

namespace FinanceManagement.Infrastructure.Interface
{
    public interface IGenerateToken
    {
        Task GenerateTokenAsync(User user);
    }
}
