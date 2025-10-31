using OpenList.Core.Interfaces;
using OpenList.Core.Models;

namespace OpenList.Infrastructure.Drivers;

/// <summary>
/// 存储驱动抽象基类
/// </summary>
public abstract class BaseStorageDriver : IStorageDriver
{
    public abstract string Name { get; }
    public string Config { get; set; } = string.Empty;

    public abstract Task InitAsync(string configJson, CancellationToken cancellationToken = default);
    public abstract Task<FileList> ListAsync(string path, CancellationToken cancellationToken = default);
    public abstract Task<FileItem?> GetAsync(string path, CancellationToken cancellationToken = default);
    public abstract Task<FileStreamInfo> GetFileStreamAsync(string path, CancellationToken cancellationToken = default);
    public abstract Task<bool> UploadAsync(string path, Stream stream, CancellationToken cancellationToken = default);
    public abstract Task<bool> MakeDirAsync(string path, CancellationToken cancellationToken = default);
    public abstract Task<bool> MoveAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default);
    public abstract Task<bool> CopyAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default);
    public abstract Task<bool> RemoveAsync(string path, CancellationToken cancellationToken = default);
    public abstract Task<bool> RenameAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 规范化路径
    /// </summary>
    protected string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "/";
        }

        path = path.Replace("\\", "/");
        
        if (!path.StartsWith("/"))
        {
            path = "/" + path;
        }

        // 移除末尾的斜杠（除非是根路径）
        if (path.Length > 1 && path.EndsWith("/"))
        {
            path = path.TrimEnd('/');
        }

        return path;
    }
}
