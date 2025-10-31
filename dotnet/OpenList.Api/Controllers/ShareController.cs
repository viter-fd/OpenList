using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenList.Application.DTOs;
using OpenList.Application.Interfaces;

namespace OpenList.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShareController : ControllerBase
{
    private readonly IShareService _shareService;
    private readonly IFileSystemService _fileSystemService;

    public ShareController(IShareService shareService, IFileSystemService fileSystemService)
    {
        _shareService = shareService;
        _fileSystemService = fileSystemService;
    }

    /// <summary>
    /// 创建分享
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ShareDto>>> CreateShare([FromBody] CreateShareRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<ShareDto>.Fail("无效的用户令牌", 401));
            }

            var share = await _shareService.CreateShareAsync(
                request.Path,
                userId,
                request.Password,
                request.ExpiresAt,
                request.MaxDownloads
            );

            return Ok(ApiResponse<ShareDto>.Ok(share, "分享创建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ShareDto>.Fail($"创建分享失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 获取分享信息
    /// </summary>
    [HttpGet("{code}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ShareDto>>> GetShare(string code, [FromQuery] string? password = null)
    {
        try
        {
            var share = await _shareService.GetShareByCodeAsync(code, password);
            
            if (share == null)
            {
                return NotFound(ApiResponse<ShareDto>.Fail("分享不存在或已失效", 404));
            }

            return Ok(ApiResponse<ShareDto>.Ok(share));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<ShareDto>.Fail(ex.Message, 401));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ShareDto>.Fail($"获取分享失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 访问分享文件
    /// </summary>
    [HttpPost("access")]
    [AllowAnonymous]
    public async Task<IActionResult> AccessShare([FromBody] AccessShareRequest request)
    {
        try
        {
            var share = await _shareService.GetShareByCodeAsync(request.Code, request.Password);
            
            if (share == null)
            {
                return NotFound(new { error = "分享不存在或已失效" });
            }

            // 增加下载次数
            await _shareService.IncrementDownloadAsync(request.Code);

            // 获取文件
            var fileStream = await _fileSystemService.GetAsync(share.Path);
            return File(fileStream.Stream, fileStream.ContentType, fileStream.FileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"访问分享失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取用户的所有分享
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<ShareDto>>>> GetMyShares()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<List<ShareDto>>.Fail("无效的用户令牌", 401));
            }

            var shares = await _shareService.GetUserSharesAsync(userId);
            return Ok(ApiResponse<List<ShareDto>>.Ok(shares));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<ShareDto>>.Fail($"获取分享列表失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 删除分享
    /// </summary>
    [HttpDelete("{code}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteShare(string code)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<bool>.Fail("无效的用户令牌", 401));
            }

            var result = await _shareService.DeleteShareAsync(code, userId);
            
            if (!result)
            {
                return NotFound(ApiResponse<bool>.Fail("分享不存在或无权删除", 404));
            }

            return Ok(ApiResponse<bool>.Ok(result, "分享已删除"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Fail($"删除分享失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 启用/禁用分享
    /// </summary>
    [HttpPatch("{code}/toggle")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleShare(string code)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<bool>.Fail("无效的用户令牌", 401));
            }

            var isEnabled = await _shareService.ToggleShareAsync(code, userId);
            var message = isEnabled ? "分享已启用" : "分享已禁用";
            
            return Ok(ApiResponse<bool>.Ok(isEnabled, message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Fail($"操作失败: {ex.Message}"));
        }
    }
}
