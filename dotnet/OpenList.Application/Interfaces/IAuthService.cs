using OpenList.Application.DTOs;

namespace OpenList.Application.Interfaces;

/// <summary>
/// 认证服务接口
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 用户登录
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// 创建用户
    /// </summary>
    Task<UserDto> CreateUserAsync(string username, string password, string displayName, string email, string role);

    /// <summary>
    /// 验证令牌
    /// </summary>
    Task<bool> ValidateTokenAsync(string token);
    
    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(int userId);
    
    /// <summary>
    /// 修改密码
    /// </summary>
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
}
