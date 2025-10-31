using OpenList.Core.Entities;

namespace OpenList.Core.Interfaces;

/// <summary>
/// 存储仓储接口
/// </summary>
public interface IStorageRepository : IRepository<Storage>
{
    /// <summary>
    /// 根据挂载路径获取存储
    /// </summary>
    Task<Storage?> GetByMountPathAsync(string mountPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有已启用的存储
    /// </summary>
    Task<IEnumerable<Storage>> GetEnabledStoragesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据用户获取存储列表
    /// </summary>
    Task<IEnumerable<Storage>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
}
