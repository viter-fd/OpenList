using System.Linq.Expressions;

namespace OpenList.Core.Interfaces;

/// <summary>
/// 仓储基接口
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    Task<T?> GetByIdAsync(long id);

    /// <summary>
    /// 获取所有实体
    /// </summary>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// 根据条件查找
    /// </summary>
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// 根据条件查找第一个
    /// </summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// 添加实体
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// 批量添加
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// 更新实体
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// 删除实体
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// 批量删除
    /// </summary>
    void DeleteRange(IEnumerable<T> entities);

    /// <summary>
    /// 计数
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 是否存在
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
