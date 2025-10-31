using System.Text.Json;
using OpenList.Core.Models;

namespace OpenList.Infrastructure.Drivers;

/// <summary>
/// 本地存储驱动配置
/// </summary>
public class LocalStorageConfig
{
    /// <summary>
    /// 根路径
    /// </summary>
    public string RootPath { get; set; } = string.Empty;
}

/// <summary>
/// 本地存储驱动
/// </summary>
public class LocalStorageDriver : BaseStorageDriver
{
    private LocalStorageConfig? _config;

    public override string Name => "local";

    public override Task InitAsync(string configJson, CancellationToken cancellationToken = default)
    {
        _config = JsonSerializer.Deserialize<LocalStorageConfig>(configJson);
        if (_config == null || string.IsNullOrEmpty(_config.RootPath))
        {
            throw new InvalidOperationException("Invalid configuration for local storage driver");
        }

        // 确保根路径存在
        if (!Directory.Exists(_config.RootPath))
        {
            Directory.CreateDirectory(_config.RootPath);
        }

        Config = configJson;
        return Task.CompletedTask;
    }

    public override Task<FileList> ListAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Driver not initialized");
        }

        var normalizedPath = NormalizePath(path);
        var fullPath = GetFullPath(normalizedPath);

        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

        var fileList = new FileList();
        var dirInfo = new DirectoryInfo(fullPath);

        // 列出子目录
        foreach (var dir in dirInfo.GetDirectories())
        {
            fileList.Items.Add(new FileItem
            {
                Name = dir.Name,
                Size = 0,
                IsDirectory = true,
                Modified = dir.LastWriteTime,
                Type = 1
            });
        }

        // 列出文件
        foreach (var file in dirInfo.GetFiles())
        {
            fileList.Items.Add(new FileItem
            {
                Name = file.Name,
                Size = file.Length,
                IsDirectory = false,
                Modified = file.LastWriteTime,
                Type = GetFileType(file.Extension)
            });
        }

        fileList.Total = fileList.Items.Count;
        fileList.Write = true;

        return Task.FromResult(fileList);
    }

    public override Task<FileItem?> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Driver not initialized");
        }

        var normalizedPath = NormalizePath(path);
        var fullPath = GetFullPath(normalizedPath);

        if (Directory.Exists(fullPath))
        {
            var dirInfo = new DirectoryInfo(fullPath);
            return Task.FromResult<FileItem?>(new FileItem
            {
                Name = dirInfo.Name,
                Size = 0,
                IsDirectory = true,
                Modified = dirInfo.LastWriteTime,
                Type = 1
            });
        }

        if (File.Exists(fullPath))
        {
            var fileInfo = new FileInfo(fullPath);
            return Task.FromResult<FileItem?>(new FileItem
            {
                Name = fileInfo.Name,
                Size = fileInfo.Length,
                IsDirectory = false,
                Modified = fileInfo.LastWriteTime,
                Type = GetFileType(fileInfo.Extension)
            });
        }

        return Task.FromResult<FileItem?>(null);
    }

    public override Task<FileStreamInfo> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Driver not initialized");
        }

        var normalizedPath = NormalizePath(path);
        var fullPath = GetFullPath(normalizedPath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        var fileInfo = new FileInfo(fullPath);
        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return Task.FromResult(new FileStreamInfo
        {
            Stream = stream,
            MimeType = GetMimeType(fileInfo.Extension),
            FileName = fileInfo.Name
        });
    }

    public override async Task<bool> UploadAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Driver not initialized");
        }

        var normalizedPath = NormalizePath(path);
        var fullPath = GetFullPath(normalizedPath);

        // 确保目录存在
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return true;
    }

    public override Task<bool> MakeDirAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Driver not initialized");
        }

        var normalizedPath = NormalizePath(path);
        var fullPath = GetFullPath(normalizedPath);

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return Task.FromResult(true);
    }

    public override Task<bool> MoveAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Driver not initialized");
        }

        var srcFullPath = GetFullPath(NormalizePath(srcPath));
        var dstFullPath = GetFullPath(NormalizePath(dstPath));

        if (File.Exists(srcFullPath))
        {
            File.Move(srcFullPath, dstFullPath);
        }
        else if (Directory.Exists(srcFullPath))
        {
            Directory.Move(srcFullPath, dstFullPath);
        }
        else
        {
            throw new FileNotFoundException($"Source not found: {srcPath}");
        }

        return Task.FromResult(true);
    }

    public override Task<bool> CopyAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Driver not initialized");
        }

        var srcFullPath = GetFullPath(NormalizePath(srcPath));
        var dstFullPath = GetFullPath(NormalizePath(dstPath));

        if (File.Exists(srcFullPath))
        {
            File.Copy(srcFullPath, dstFullPath, true);
        }
        else if (Directory.Exists(srcFullPath))
        {
            CopyDirectory(srcFullPath, dstFullPath);
        }
        else
        {
            throw new FileNotFoundException($"Source not found: {srcPath}");
        }

        return Task.FromResult(true);
    }

    public override Task<bool> RemoveAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Driver not initialized");
        }

        var fullPath = GetFullPath(NormalizePath(path));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        else if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);
        }
        else
        {
            throw new FileNotFoundException($"Path not found: {path}");
        }

        return Task.FromResult(true);
    }

    public override Task<bool> RenameAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        return MoveAsync(srcPath, dstPath, cancellationToken);
    }

    private string GetFullPath(string path)
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Driver not initialized");
        }

        // 移除开头的斜杠
        if (path.StartsWith("/"))
        {
            path = path.Substring(1);
        }

        return Path.Combine(_config.RootPath, path);
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var dest = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, dest, true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var dest = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, dest);
        }
    }

    private static int GetFileType(string extension)
    {
        extension = extension.ToLowerInvariant();
        
        if (new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" }.Contains(extension))
            return 2; // 图片

        if (new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm" }.Contains(extension))
            return 3; // 视频

        if (new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a" }.Contains(extension))
            return 4; // 音频

        if (new[] { ".txt", ".md", ".log" }.Contains(extension))
            return 5; // 文本

        return 0; // 其他
    }

    private static string GetMimeType(string extension)
    {
        extension = extension.ToLowerInvariant();

        var mimeTypes = new Dictionary<string, string>
        {
            { ".txt", "text/plain" },
            { ".html", "text/html" },
            { ".css", "text/css" },
            { ".js", "application/javascript" },
            { ".json", "application/json" },
            { ".xml", "application/xml" },
            { ".pdf", "application/pdf" },
            { ".zip", "application/zip" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".svg", "image/svg+xml" },
            { ".mp4", "video/mp4" },
            { ".mp3", "audio/mpeg" },
            { ".wav", "audio/wav" }
        };

        return mimeTypes.TryGetValue(extension, out var mimeType) 
            ? mimeType 
            : "application/octet-stream";
    }
}
