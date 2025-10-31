using Microsoft.EntityFrameworkCore;
using OpenList.Core.Entities;
using OpenList.Core.Interfaces;
using OpenList.Infrastructure.Data;

namespace OpenList.Infrastructure.Repositories;

public class ShareRepository : Repository<Share>, IShareRepository
{
    public ShareRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Share?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Code == code);
    }

    public async Task<List<Share>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> CodeExistsAsync(string code)
    {
        return await _dbSet.AnyAsync(s => s.Code == code);
    }
}
