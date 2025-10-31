using OpenList.Core.Models;

namespace OpenList.Application.Interfaces;

/// <summary>
/// 文件系统服务接口
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// 列出文件
    /// </summary>
    Task<FileList> ListAsync(string path);

    /// <summary>
    /// 获取文件流
    /// </summary>
    Task<FileStreamInfo> GetAsync(string path);

    /// <summary>
    /// 上传文件
    /// </summary>
    Task<bool> UploadAsync(string path, Stream content, string fileName);

    /// <summary>
    /// 创建目录
    /// </summary>
    Task<bool> MakeDirAsync(string path);

    /// <summary>
    /// 移动文件
    /// </summary>
    Task<bool> MoveAsync(string sourcePath, string destPath);

    /// <summary>
    /// 复制文件
    /// </summary>
    Task<bool> CopyAsync(string sourcePath, string destPath);

    /// <summary>
    /// 删除文件/目录
    /// </summary>
    Task<bool> RemoveAsync(string path);

    /// <summary>
    /// 重命名
    /// </summary>
    Task<bool> RenameAsync(string path, string newName);

    /// <summary>
    /// 搜索文件
    /// </summary>
    Task<List<FileItem>> SearchAsync(string keyword, string? mountPath = null);
}
