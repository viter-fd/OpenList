namespace OpenList.Application.DTOs;

/// <summary>
/// 用户登录请求
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 用户名
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// 2FA代码
    /// </summary>
    public string? TwoFactorCode { get; set; }
}

/// <summary>
/// 登录响应
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public required string AccessToken { get; set; }
    
    /// <summary>
    /// Token (兼容属性)
    /// </summary>
    public string Token
    {
        get => AccessToken;
        set => AccessToken = value;
    }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 过期时间（秒）
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    public UserDto? User { get; set; }
}

/// <summary>
/// 用户DTO
/// </summary>
public class UserDto
{
    public long Id { get; set; }
    public required string Username { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = "user";
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsEnabled { get; set; }
    public bool TwoFactorEnabled { get; set; }
}

/// <summary>
/// 创建用户请求
/// </summary>
public class CreateUserRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = "user";
}

/// <summary>
/// 文件列表响应
/// </summary>
public class FileListResponse
{
    public List<FileItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public string? Readme { get; set; }
    public string? Header { get; set; }
    public bool Write { get; set; }
}

/// <summary>
/// 文件项DTO
/// </summary>
public class FileItemDto
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public long Size { get; set; }
    public bool IsDirectory { get; set; }
    public DateTime ModifiedTime { get; set; }
    public string? FileType { get; set; }
    public string? MimeType { get; set; }
    public string? Sign { get; set; }
    public string? Thumbnail { get; set; }
}

/// <summary>
/// 存储DTO
/// </summary>
public class StorageDto
{
    public long Id { get; set; }
    public required string MountPath { get; set; }
    public required string Name { get; set; }
    public required string Driver { get; set; }
    public string ConfigJson { get; set; } = "{}";
    public int Order { get; set; }
    public bool IsEnabled { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 创建存储请求
/// </summary>
public class CreateStorageRequest
{
    public required string MountPath { get; set; }
    public required string Name { get; set; }
    public required string Driver { get; set; }
    public required string ConfigJson { get; set; }
    public int Order { get; set; }
    public string? Remark { get; set; }
}

/// <summary>
/// API响应包装
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 错误代码
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 成功响应
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    /// <summary>
    /// 失败响应
    /// </summary>
    public static ApiResponse<T> Fail(string message, int? statusCode = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorCode = statusCode?.ToString()
        };
    }
}
