using OpenList.Core.Entities;

namespace OpenList.Core.Interfaces;

public interface IShareRepository : IRepository<Share>
{
    Task<Share?> GetByCodeAsync(string code);
    Task<List<Share>> GetByUserIdAsync(int userId);
    Task<bool> CodeExistsAsync(string code);
}
