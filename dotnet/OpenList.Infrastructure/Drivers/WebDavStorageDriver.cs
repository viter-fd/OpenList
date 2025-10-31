using System.Net;
using System.Text.Json;
using WebDav;
using OpenList.Core.Models;

namespace OpenList.Infrastructure.Drivers;

public class WebDavStorageDriver : BaseStorageDriver
{
    private WebDavClient? _client;
    private string _baseUrl = string.Empty;

    public override string Name => "WebDAV";

    public override async Task InitAsync(string configJson, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            var config = JsonDocument.Parse(configJson).RootElement;
            
            _baseUrl = config.GetProperty("Url").GetString() 
                ?? throw new InvalidOperationException("Url is required");
            var username = config.TryGetProperty("Username", out var user) 
                ? user.GetString() : null;
            var password = config.TryGetProperty("Password", out var pass) 
                ? pass.GetString() : null;

            var clientParams = new WebDavClientParams
            {
                BaseAddress = new Uri(_baseUrl)
            };

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                clientParams.Credentials = new NetworkCredential(username, password);
            }

            _client = new WebDavClient(clientParams);
            Config = configJson;
        }, cancellationToken);
    }

    public override async Task<FileList> ListAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var result = await _client.Propfind(GetFullUrl(path), new PropfindParameters { CancellationToken = cancellationToken });
        if (!result.IsSuccessful)
        {
            throw new InvalidOperationException($"Failed to list directory: {result.Description}");
        }

        var items = new List<FileItem>();
        foreach (var resource in result.Resources.Skip(1)) // 跳过第一个（当前目录）
        {
            var name = resource.Uri.TrimEnd('/').Split('/').Last();
            if (string.IsNullOrEmpty(name)) continue;

            items.Add(new FileItem
            {
                Name = Uri.UnescapeDataString(name),
                Path = $"{path.TrimEnd('/')}/{Uri.UnescapeDataString(name)}",
                IsDirectory = resource.IsCollection,
                Size = resource.ContentLength ?? 0,
                ModifiedTime = resource.LastModifiedDate ?? DateTime.UtcNow
            });
        }

        return new FileList
        {
            Items = items,
            Path = path,
            Total = items.Count
        };
    }

    public override async Task<FileItem?> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var result = await _client.Propfind(GetFullUrl(path), new PropfindParameters { CancellationToken = cancellationToken });
        if (!result.IsSuccessful || !result.Resources.Any())
        {
            return null;
        }

        var resource = result.Resources.First();
        return new FileItem
        {
            Name = Path.GetFileName(path),
            Path = path,
            IsDirectory = resource.IsCollection,
            Size = resource.ContentLength ?? 0,
            ModifiedTime = resource.LastModifiedDate ?? DateTime.UtcNow
        };
    }

    public override async Task<FileStreamInfo> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var response = await _client.GetRawFile(GetFullUrl(path), new GetFileParameters { CancellationToken = cancellationToken });
        if (!response.IsSuccessful)
        {
            throw new InvalidOperationException($"Failed to get file: {response.Description}");
        }

        return new FileStreamInfo
        {
            Stream = response.Stream,
            MimeType = "application/octet-stream",
            FileName = Path.GetFileName(path)
        };
    }

    public override async Task<bool> UploadAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var result = await _client.PutFile(GetFullUrl(path), stream, new PutFileParameters { CancellationToken = cancellationToken });
        return result.IsSuccessful;
    }

    public override async Task<bool> MakeDirAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var result = await _client.Mkcol(GetFullUrl(path), new MkColParameters { CancellationToken = cancellationToken });
        return result.IsSuccessful;
    }

    public override async Task<bool> RemoveAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var result = await _client.Delete(GetFullUrl(path), new DeleteParameters { CancellationToken = cancellationToken });
        return result.IsSuccessful;
    }

    public override async Task<bool> RenameAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var result = await _client.Move(GetFullUrl(srcPath), GetFullUrl(dstPath), new MoveParameters { CancellationToken = cancellationToken });
        return result.IsSuccessful;
    }

    public override async Task<bool> MoveAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        return await RenameAsync(srcPath, dstPath, cancellationToken);
    }

    public override async Task<bool> CopyAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var result = await _client.Copy(GetFullUrl(srcPath), GetFullUrl(dstPath), new CopyParameters { CancellationToken = cancellationToken });
        return result.IsSuccessful;
    }

    private string GetFullUrl(string path)
    {
        return $"{_baseUrl.TrimEnd('/')}{NormalizePath(path)}";
    }
}
