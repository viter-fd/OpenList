using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenList.Application.DTOs;
using OpenList.Core.Entities;
using OpenList.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace OpenList.Api.Controllers;

/// <summary>
/// 用户控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<UserController> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有用户
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAll()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Email = u.Email,
                Role = u.Role.ToString().ToLower(),
                IsEnabled = u.IsEnabled,
                TwoFactorEnabled = u.TwoFactorEnabled
            });

            return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(userDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, ApiResponse<IEnumerable<UserDto>>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(long id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.Fail("User not found"));
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Role = user.Role.ToString().ToLower(),
                IsEnabled = user.IsEnabled,
                TwoFactorEnabled = user.TwoFactorEnabled
            };

            return Ok(ApiResponse<UserDto>.Ok(userDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {Id}", id);
            return StatusCode(500, ApiResponse<UserDto>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            // 检查用户名是否已存在
            if (await _userRepository.UsernameExistsAsync(request.Username))
            {
                return BadRequest(ApiResponse<UserDto>.Fail("Username already exists"));
            }

            // 生成盐值和密码哈希
            var salt = GenerateSalt();
            var passwordHash = HashPassword(request.Password, salt);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Salt = salt,
                DisplayName = request.DisplayName,
                Email = request.Email,
                Role = Enum.Parse<UserRole>(request.Role, true),
                IsEnabled = true
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Role = user.Role.ToString().ToLower(),
                IsEnabled = user.IsEnabled,
                TwoFactorEnabled = user.TwoFactorEnabled
            };

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, ApiResponse<UserDto>.Ok(userDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, ApiResponse<UserDto>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<bool>.Fail("User not found"));
            }

            _userRepository.Delete(user);
            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "User deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
        }
    }

    private static string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var combined = new byte[saltBytes.Length + passwordBytes.Length];
        
        Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
        Buffer.BlockCopy(passwordBytes, 0, combined, saltBytes.Length, passwordBytes.Length);

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(combined);
        return Convert.ToBase64String(hash);
    }
}
