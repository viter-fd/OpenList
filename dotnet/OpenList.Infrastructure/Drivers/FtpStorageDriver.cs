using System.Text.Json;
using FluentFTP;
using OpenList.Core.Models;

namespace OpenList.Infrastructure.Drivers;

public class FtpStorageDriver : BaseStorageDriver
{
    private AsyncFtpClient? _client;

    public override string Name => "FTP";

    public override async Task InitAsync(string configJson, CancellationToken cancellationToken = default)
    {
        var config = JsonDocument.Parse(configJson).RootElement;
        
        var host = config.GetProperty("Host").GetString() 
            ?? throw new InvalidOperationException("Host is required");
        var port = config.TryGetProperty("Port", out var p) ? p.GetInt32() : 21;
        var username = config.TryGetProperty("Username", out var user) 
            ? user.GetString() : "anonymous";
        var password = config.TryGetProperty("Password", out var pass) 
            ? pass.GetString() : "";

        _client = new AsyncFtpClient(host, username, password, port);
        _client.Config.EncryptionMode = FtpEncryptionMode.Auto;
        _client.Config.ValidateAnyCertificate = true;
        
        Config = configJson;
        await Task.CompletedTask;
    }

    public override async Task<FileList> ListAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        await EnsureConnectedAsync(cancellationToken);
        
        var items = await _client.GetListing(NormalizePath(path), cancellationToken);
        var fileItems = new List<FileItem>();

        foreach (var item in items)
        {
            if (item.Name == "." || item.Name == "..") continue;

            fileItems.Add(new FileItem
            {
                Name = item.Name,
                Path = $"{path.TrimEnd('/')}/{item.Name}",
                IsDirectory = item.Type == FtpObjectType.Directory,
                Size = item.Size,
                ModifiedTime = item.Modified
            });
        }

        return new FileList
        {
            Items = fileItems,
            Path = path,
            Total = fileItems.Count
        };
    }

    public override async Task<FileItem?> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        await EnsureConnectedAsync(cancellationToken);
        
        try
        {
            var item = await _client.GetObjectInfo(NormalizePath(path), token: cancellationToken);
            if (item == null) return null;
            
            return new FileItem
            {
                Name = item.Name,
                Path = path,
                IsDirectory = item.Type == FtpObjectType.Directory,
                Size = item.Size,
                ModifiedTime = item.Modified
            };
        }
        catch
        {
            return null;
        }
    }

    public override async Task<FileStreamInfo> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        await EnsureConnectedAsync(cancellationToken);

        var stream = new MemoryStream();
        await _client.DownloadStream(stream, NormalizePath(path), token: cancellationToken);
        stream.Position = 0;

        return new FileStreamInfo
        {
            Stream = stream,
            MimeType = "application/octet-stream",
            FileName = Path.GetFileName(path)
        };
    }

    public override async Task<bool> UploadAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        await EnsureConnectedAsync(cancellationToken);

        var result = await _client.UploadStream(stream, NormalizePath(path), FtpRemoteExists.Overwrite, token: cancellationToken);
        return result == FtpStatus.Success;
    }

    public override async Task<bool> MakeDirAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        await EnsureConnectedAsync(cancellationToken);

        await _client.CreateDirectory(NormalizePath(path), cancellationToken);
        return true;
    }

    public override async Task<bool> RemoveAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        await EnsureConnectedAsync(cancellationToken);

        var item = await _client.GetObjectInfo(NormalizePath(path), token: cancellationToken);
        if (item.Type == FtpObjectType.Directory)
        {
            await _client.DeleteDirectory(NormalizePath(path), cancellationToken);
        }
        else
        {
            await _client.DeleteFile(NormalizePath(path), cancellationToken);
        }

        return true;
    }

    public override async Task<bool> RenameAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        await EnsureConnectedAsync(cancellationToken);

        return await _client.Rename(NormalizePath(srcPath), NormalizePath(dstPath), cancellationToken);
    }

    public override async Task<bool> MoveAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        return await RenameAsync(srcPath, dstPath, cancellationToken);
    }

    public override async Task<bool> CopyAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        await EnsureConnectedAsync(cancellationToken);

        // FTP 不直接支持复制，需要先下载再上传
        var stream = new MemoryStream();
        await _client.DownloadStream(stream, NormalizePath(srcPath), token: cancellationToken);
        stream.Position = 0;
        
        var result = await _client.UploadStream(stream, NormalizePath(dstPath), FtpRemoteExists.Overwrite, token: cancellationToken);
        return result == FtpStatus.Success;
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        if (_client != null && !_client.IsConnected)
        {
            await _client.Connect(cancellationToken);
        }
    }
}
