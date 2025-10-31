using OpenList.Core.Models;

namespace OpenList.Core.Interfaces;

/// <summary>
/// 存储驱动接口
/// </summary>
public interface IStorageDriver
{
    /// <summary>
    /// 驱动名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 配置
    /// </summary>
    string Config { get; set; }

    /// <summary>
    /// 初始化驱动
    /// </summary>
    Task InitAsync(string configJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// 列出文件
    /// </summary>
    Task<FileList> ListAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取文件
    /// </summary>
    Task<FileItem?> GetAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取文件流
    /// </summary>
    Task<FileStreamInfo> GetFileStreamAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// 上传文件
    /// </summary>
    Task<bool> UploadAsync(string path, Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建目录
    /// </summary>
    Task<bool> MakeDirAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移动文件/目录
    /// </summary>
    Task<bool> MoveAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 复制文件/目录
    /// </summary>
    Task<bool> CopyAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除文件/目录
    /// </summary>
    Task<bool> RemoveAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// 重命名
    /// </summary>
    Task<bool> RenameAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default);
}
