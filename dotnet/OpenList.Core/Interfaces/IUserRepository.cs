using OpenList.Core.Entities;

namespace OpenList.Core.Interfaces;

/// <summary>
/// 用户仓储接口
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据邮箱获取用户
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户名是否存在
    /// </summary>
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
}
