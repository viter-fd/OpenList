using Microsoft.AspNetCore.Mvc;
using OpenList.Application.DTOs;

namespace OpenList.Api.Controllers;

/// <summary>
/// 系统信息控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public SystemController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// 获取系统信息
    /// </summary>
    [HttpGet("info")]
    public ActionResult<ApiResponse<object>> GetInfo()
    {
        var info = new
        {
            version = "1.0.0-dotnet",
            framework = ".NET 8.0",
            language = "C#",
            os = Environment.OSVersion.ToString(),
            startTime = DateTime.UtcNow.ToString("o")
        };

        return Ok(ApiResponse<object>.Ok(info));
    }

    /// <summary>
    /// 健康检查
    /// </summary>
    [HttpGet("health")]
    public ActionResult<ApiResponse<object>> HealthCheck()
    {
        var health = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow.ToString("o")
        };

        return Ok(ApiResponse<object>.Ok(health));
    }

    /// <summary>
    /// 获取系统版本
    /// </summary>
    [HttpGet("version")]
    public ActionResult<ApiResponse<object>> GetVersion()
    {
        var version = new
        {
            version = "1.0.0",
            buildDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            gitCommit = "dotnet-rewrite"
        };

        return Ok(ApiResponse<object>.Ok(version));
    }
}
