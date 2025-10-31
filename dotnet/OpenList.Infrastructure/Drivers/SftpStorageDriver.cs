using System.Text.Json;
using Renci.SshNet;
using OpenList.Core.Models;

namespace OpenList.Infrastructure.Drivers;

public class SftpStorageDriver : BaseStorageDriver
{
    private SftpClient? _client;

    public override string Name => "SFTP";

    public override async Task InitAsync(string configJson, CancellationToken cancellationToken = default)
    {
        var config = JsonDocument.Parse(configJson).RootElement;
        
        var host = config.GetProperty("Host").GetString() 
            ?? throw new InvalidOperationException("Host is required");
        var port = config.TryGetProperty("Port", out var p) ? p.GetInt32() : 22;
        var username = config.GetProperty("Username").GetString() 
            ?? throw new InvalidOperationException("Username is required");
        
        // 支持密码或私钥认证
        if (config.TryGetProperty("Password", out var password) && !string.IsNullOrEmpty(password.GetString()))
        {
            _client = new SftpClient(host, port, username, password.GetString());
        }
        else if (config.TryGetProperty("PrivateKey", out var privateKey) && !string.IsNullOrEmpty(privateKey.GetString()))
        {
            var keyFile = new PrivateKeyFile(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(privateKey.GetString()!)));
            _client = new SftpClient(host, port, username, keyFile);
        }
        else
        {
            throw new InvalidOperationException("Either Password or PrivateKey is required");
        }
        
        Config = configJson;
        await Task.CompletedTask;
    }

    public override async Task<FileList> ListAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            EnsureConnected();
            
            var items = _client.ListDirectory(NormalizePath(path));
            var fileItems = new List<FileItem>();

            foreach (var item in items)
            {
                if (item.Name == "." || item.Name == "..") continue;

                fileItems.Add(new FileItem
                {
                    Name = item.Name,
                    Path = $"{path.TrimEnd('/')}/{item.Name}",
                    IsDirectory = item.IsDirectory,
                    Size = item.Length,
                    ModifiedTime = item.LastWriteTime
                });
            }

            return new FileList
            {
                Items = fileItems,
                Path = path,
                Total = fileItems.Count
            };
        }, cancellationToken);
    }

    public override async Task<FileItem?> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            EnsureConnected();
            
            try
            {
                var item = _client.Get(NormalizePath(path));
                if (item == null) return null;
                
                return new FileItem
                {
                    Name = item.Name,
                    Path = path,
                    IsDirectory = item.IsDirectory,
                    Size = item.Length,
                    ModifiedTime = item.LastWriteTime
                };
            }
            catch
            {
                return null;
            }
        }, cancellationToken);
    }

    public override async Task<FileStreamInfo> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            EnsureConnected();

            var stream = new MemoryStream();
            _client.DownloadFile(NormalizePath(path), stream);
            stream.Position = 0;

            return new FileStreamInfo
            {
                Stream = stream,
                MimeType = "application/octet-stream",
                FileName = Path.GetFileName(path)
            };
        }, cancellationToken);
    }

    public override async Task<bool> UploadAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            EnsureConnected();
            _client.UploadFile(stream, NormalizePath(path), true);
            return true;
        }, cancellationToken);
    }

    public override async Task<bool> MakeDirAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            EnsureConnected();
            _client.CreateDirectory(NormalizePath(path));
            return true;
        }, cancellationToken);
    }

    public override async Task<bool> RemoveAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            EnsureConnected();
            
            var item = _client.Get(NormalizePath(path));
            if (item.IsDirectory)
            {
                DeleteDirectory(NormalizePath(path));
            }
            else
            {
                _client.DeleteFile(NormalizePath(path));
            }

            return true;
        }, cancellationToken);
    }

    public override async Task<bool> RenameAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            EnsureConnected();
            _client.RenameFile(NormalizePath(srcPath), NormalizePath(dstPath));
            return true;
        }, cancellationToken);
    }

    public override async Task<bool> MoveAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        return await RenameAsync(srcPath, dstPath, cancellationToken);
    }

    public override async Task<bool> CopyAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            EnsureConnected();
            
            // SFTP 不直接支持复制，需要先读取再写入
            using var stream = new MemoryStream();
            _client.DownloadFile(NormalizePath(srcPath), stream);
            stream.Position = 0;
            _client.UploadFile(stream, NormalizePath(dstPath), true);
            
            return true;
        }, cancellationToken);
    }

    private void EnsureConnected()
    {
        if (_client != null && !_client.IsConnected)
        {
            _client.Connect();
        }
    }

    private void DeleteDirectory(string path)
    {
        if (_client == null) return;

        var items = _client.ListDirectory(path);
        foreach (var item in items)
        {
            if (item.Name == "." || item.Name == "..") continue;

            if (item.IsDirectory)
            {
                DeleteDirectory(item.FullName);
            }
            else
            {
                _client.DeleteFile(item.FullName);
            }
        }

        _client.DeleteDirectory(path);
    }
}
