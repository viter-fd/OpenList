namespace OpenList.Core.Interfaces;

/// <summary>
/// 工作单元接口
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// 用户仓储
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// 存储仓储
    /// </summary>
    IStorageRepository Storages { get; }

    /// <summary>
    /// 保存更改
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 开始事务
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交事务
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 回滚事务
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
