using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenList.Application.DTOs;
using OpenList.Application.Interfaces;

namespace OpenList.Api.Controllers;

[ApiController]
[Route("api/fs")]
[Authorize]
public class FileSystemController : ControllerBase
{
    private readonly IFileSystemService _fileSystemService;

    public FileSystemController(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    /// <summary>
    /// 列出目录内容
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<FileListDto>>> List([FromQuery] string path = "/")
    {
        try
        {
            var result = await _fileSystemService.ListAsync(path);
            var dto = new FileListDto
            {
                Path = result.Path,
                Items = result.Items.Select(i => new FileItemDto
                {
                    Name = i.Name,
                    Path = i.Path,
                    IsDirectory = i.IsDirectory,
                    Size = i.Size,
                    ModifiedTime = i.ModifiedTime,
                    FileType = i.FileType.ToString(),
                    MimeType = i.MimeType
                }).ToList()
            };

            return Ok(ApiResponse<FileListDto>.Ok(dto));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<FileListDto>.Fail($"列出目录失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string path)
    {
        try
        {
            var fileStream = await _fileSystemService.GetAsync(path);
            return File(fileStream.Stream, fileStream.ContentType, fileStream.FileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"下载文件失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<bool>>> Upload([FromQuery] string path, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<bool>.Fail("请选择要上传的文件"));
            }

            using var stream = file.OpenReadStream();
            var result = await _fileSystemService.UploadAsync(path, stream, file.FileName);
            
            return Ok(ApiResponse<bool>.Ok(result, "文件上传成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Fail($"上传文件失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 批量上传文件
    /// </summary>
    [HttpPost("upload-multiple")]
    public async Task<ActionResult<ApiResponse<UploadResultDto>>> UploadMultiple([FromQuery] string path, List<IFormFile> files)
    {
        try
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(ApiResponse<UploadResultDto>.Fail("请选择要上传的文件"));
            }

            var results = new UploadResultDto
            {
                Total = files.Count,
                Success = 0,
                Failed = 0,
                Details = new List<UploadDetailDto>()
            };

            foreach (var file in files)
            {
                try
                {
                    using var stream = file.OpenReadStream();
                    var success = await _fileSystemService.UploadAsync(path, stream, file.FileName);
                    
                    if (success)
                    {
                        results.Success++;
                        results.Details.Add(new UploadDetailDto
                        {
                            FileName = file.FileName,
                            Success = true
                        });
                    }
                    else
                    {
                        results.Failed++;
                        results.Details.Add(new UploadDetailDto
                        {
                            FileName = file.FileName,
                            Success = false,
                            Error = "上传失败"
                        });
                    }
                }
                catch (Exception ex)
                {
                    results.Failed++;
                    results.Details.Add(new UploadDetailDto
                    {
                        FileName = file.FileName,
                        Success = false,
                        Error = ex.Message
                    });
                }
            }

            return Ok(ApiResponse<UploadResultDto>.Ok(results, 
                $"上传完成: 成功 {results.Success} 个, 失败 {results.Failed} 个"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UploadResultDto>.Fail($"批量上传失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    [HttpPost("mkdir")]
    public async Task<ActionResult<ApiResponse<bool>>> MakeDirectory([FromBody] PathRequest request)
    {
        try
        {
            var result = await _fileSystemService.MakeDirAsync(request.Path);
            return Ok(ApiResponse<bool>.Ok(result, "目录创建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Fail($"创建目录失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 删除文件或目录
    /// </summary>
    [HttpDelete("remove")]
    public async Task<ActionResult<ApiResponse<bool>>> Remove([FromQuery] string path)
    {
        try
        {
            var result = await _fileSystemService.RemoveAsync(path);
            return Ok(ApiResponse<bool>.Ok(result, "删除成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Fail($"删除失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 重命名文件或目录
    /// </summary>
    [HttpPost("rename")]
    public async Task<ActionResult<ApiResponse<bool>>> Rename([FromBody] RenameRequest request)
    {
        try
        {
            var result = await _fileSystemService.RenameAsync(request.Path, request.NewName);
            return Ok(ApiResponse<bool>.Ok(result, "重命名成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Fail($"重命名失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 移动文件或目录
    /// </summary>
    [HttpPost("move")]
    public async Task<ActionResult<ApiResponse<bool>>> Move([FromBody] MoveRequest request)
    {
        try
        {
            var result = await _fileSystemService.MoveAsync(request.SourcePath, request.DestPath);
            return Ok(ApiResponse<bool>.Ok(result, "移动成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Fail($"移动失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 复制文件或目录
    /// </summary>
    [HttpPost("copy")]
    public async Task<ActionResult<ApiResponse<bool>>> Copy([FromBody] CopyRequest request)
    {
        try
        {
            var result = await _fileSystemService.CopyAsync(request.SourcePath, request.DestPath);
            return Ok(ApiResponse<bool>.Ok(result, "复制成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Fail($"复制失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 搜索文件
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<FileItemDto>>>> Search(
        [FromQuery] string keyword,
        [FromQuery] string? mountPath = null)
    {
        try
        {
            var results = await _fileSystemService.SearchAsync(keyword, mountPath);
            var dtos = results.Select(i => new FileItemDto
            {
                Name = i.Name,
                Path = i.Path,
                IsDirectory = i.IsDirectory,
                Size = i.Size,
                ModifiedTime = i.ModifiedTime,
                FileType = i.FileType.ToString(),
                MimeType = i.MimeType
            }).ToList();

            return Ok(ApiResponse<List<FileItemDto>>.Ok(dtos, $"找到 {dtos.Count} 个结果"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<FileItemDto>>.Fail($"搜索失败: {ex.Message}"));
        }
    }
}

// 请求DTOs
public record PathRequest(string Path);
public record RenameRequest(string Path, string NewName);
public record MoveRequest(string SourcePath, string DestPath);
public record CopyRequest(string SourcePath, string DestPath);

// 响应DTOs
public class FileListDto
{
    public string Path { get; set; } = string.Empty;
    public List<FileItemDto> Items { get; set; } = new();
}

public class UploadResultDto
{
    public int Total { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    public List<UploadDetailDto> Details { get; set; } = new();
}

public class UploadDetailDto
{
    public string FileName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
}
