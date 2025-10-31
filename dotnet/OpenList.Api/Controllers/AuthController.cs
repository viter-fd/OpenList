using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenList.Application.DTOs;
using OpenList.Application.Interfaces;

namespace OpenList.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(ApiResponse<LoginResponse>.Ok(response, "登录成功"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail(ex.Message, 401));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<LoginResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _authService.CreateUserAsync(
                request.Username,
                request.Password,
                request.DisplayName ?? request.Username,
                request.Email ?? string.Empty,
                "user"
            );

            return Ok(ApiResponse<UserDto>.Ok(user, "注册成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserDto>.Fail($"注册失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<UserDto>.Fail("无效的用户令牌", 401));
            }

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.Fail("用户不存在", 404));
            }

            return Ok(ApiResponse<UserDto>.Ok(user));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserDto>.Fail($"获取用户信息失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<bool>.Fail("无效的用户令牌", 401));
            }

            var result = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
            return Ok(ApiResponse<bool>.Ok(result, "密码修改成功"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<bool>.Fail(ex.Message, 401));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.Fail($"修改密码失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 验证令牌
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateToken([FromBody] TokenValidationRequest request)
    {
        try
        {
            var isValid = await _authService.ValidateTokenAsync(request.Token);
            return Ok(ApiResponse<bool>.Ok(isValid, isValid ? "令牌有效" : "令牌无效"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Fail($"验证失败: {ex.Message}"));
        }
    }
}

// 额外的请求DTOs
public record RegisterRequest(
    string Username,
    string Password,
    string? DisplayName,
    string? Email
);

public record ChangePasswordRequest(
    string OldPassword,
    string NewPassword
);

public record TokenValidationRequest(
    string Token
);
