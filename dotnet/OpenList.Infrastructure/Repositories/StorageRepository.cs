using Microsoft.EntityFrameworkCore;
using OpenList.Core.Entities;
using OpenList.Core.Interfaces;
using OpenList.Infrastructure.Data;

namespace OpenList.Infrastructure.Repositories;

/// <summary>
/// 存储仓储实现
/// </summary>
public class StorageRepository : Repository<Storage>, IStorageRepository
{
    public StorageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Storage?> GetByMountPathAsync(string mountPath, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.MountPath == mountPath, cancellationToken);
    }

    public async Task<IEnumerable<Storage>> GetEnabledStoragesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsEnabled)
            .OrderBy(s => s.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Storage>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.Order)
            .ToListAsync(cancellationToken);
    }
}
