namespace OpenList.Core.Entities;

/// <summary>
/// 用户实体
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// 用户名
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// 密码哈希
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// 盐值
    /// </summary>
    public required string Salt { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 头像URL
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 是否启用两步验证
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// 两步验证密钥
    /// </summary>
    public string? TwoFactorSecret { get; set; }

    /// <summary>
    /// 存储列表
    /// </summary>
    public ICollection<Storage> Storages { get; set; } = new List<Storage>();
}

/// <summary>
/// 用户角色枚举
/// </summary>
public enum UserRole
{
    /// <summary>
    /// 访客
    /// </summary>
    Guest = 0,

    /// <summary>
    /// 普通用户
    /// </summary>
    User = 1,

    /// <summary>
    /// 管理员
    /// </summary>
    Admin = 2
}
