using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OpenList.Core.Models;

namespace OpenList.Infrastructure.Drivers;

public class OneDriveStorageDriver : BaseStorageDriver
{
    private HttpClient? _httpClient;
    private string _accessToken = string.Empty;
    private string _refreshToken = string.Empty;
    private string _clientId = string.Empty;
    private string _clientSecret = string.Empty;
    private string _rootPath = string.Empty;
    private DateTime _tokenExpiry;

    public override string Name => "OneDrive";

    public override async Task InitAsync(string configJson, CancellationToken cancellationToken = default)
    {
        var config = JsonDocument.Parse(configJson).RootElement;
        
        _clientId = config.GetProperty("ClientId").GetString() 
            ?? throw new InvalidOperationException("ClientId is required");
        _clientSecret = config.GetProperty("ClientSecret").GetString() 
            ?? throw new InvalidOperationException("ClientSecret is required");
        _refreshToken = config.GetProperty("RefreshToken").GetString() 
            ?? throw new InvalidOperationException("RefreshToken is required");
        _rootPath = config.TryGetProperty("RootPath", out var root) 
            ? root.GetString() ?? "" : "";

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        // 获取初始访问令牌
        await RefreshAccessTokenAsync(cancellationToken);
        
        Config = configJson;
    }

    public override async Task<FileList> ListAsync(string path, CancellationToken cancellationToken = default)
    {
        await EnsureValidTokenAsync(cancellationToken);

        var fullPath = GetFullPath(path);
        var url = string.IsNullOrEmpty(fullPath) || fullPath == "/" 
            ? "me/drive/root/children" 
            : $"me/drive/root:{fullPath}:/children";

        var response = await _httpClient!.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = JsonDocument.Parse(json);
        var items = new List<FileItem>();

        foreach (var item in doc.RootElement.GetProperty("value").EnumerateArray())
        {
            var name = item.GetProperty("name").GetString() ?? "";
            var isFolder = item.TryGetProperty("folder", out _);
            var size = item.TryGetProperty("size", out var sizeElem) ? sizeElem.GetInt64() : 0;
            var modifiedStr = item.GetProperty("lastModifiedDateTime").GetString();
            var modified = DateTime.Parse(modifiedStr ?? DateTime.UtcNow.ToString());

            items.Add(new FileItem
            {
                Name = name,
                Path = $"{path.TrimEnd('/')}/{name}",
                IsDirectory = isFolder,
                Size = size,
                ModifiedTime = modified
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
        await EnsureValidTokenAsync(cancellationToken);

        var fullPath = GetFullPath(path);
        var url = $"me/drive/root:{fullPath}";

        try
        {
            var response = await _httpClient!.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var item = JsonDocument.Parse(json).RootElement;

            var name = item.GetProperty("name").GetString() ?? "";
            var isFolder = item.TryGetProperty("folder", out _);
            var size = item.TryGetProperty("size", out var sizeElem) ? sizeElem.GetInt64() : 0;
            var modifiedStr = item.GetProperty("lastModifiedDateTime").GetString();
            var modified = DateTime.Parse(modifiedStr ?? DateTime.UtcNow.ToString());

            return new FileItem
            {
                Name = name,
                Path = path,
                IsDirectory = isFolder,
                Size = size,
                ModifiedTime = modified
            };
        }
        catch
        {
            return null;
        }
    }

    public override async Task<FileStreamInfo> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        await EnsureValidTokenAsync(cancellationToken);

        var fullPath = GetFullPath(path);
        var url = $"me/drive/root:{fullPath}:/content";

        var response = await _httpClient!.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return new FileStreamInfo
        {
            Stream = memoryStream,
            MimeType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream",
            FileName = Path.GetFileName(path)
        };
    }

    public override async Task<bool> UploadAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        await EnsureValidTokenAsync(cancellationToken);

        var fullPath = GetFullPath(path);
        var url = $"me/drive/root:{fullPath}:/content";

        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var response = await _httpClient!.PutAsync(url, streamContent, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public override async Task<bool> MakeDirAsync(string path, CancellationToken cancellationToken = default)
    {
        await EnsureValidTokenAsync(cancellationToken);

        var fullPath = GetFullPath(path);
        var parentPath = Path.GetDirectoryName(fullPath)?.Replace('\\', '/') ?? "/";
        var folderName = Path.GetFileName(fullPath);

        var url = string.IsNullOrEmpty(parentPath) || parentPath == "/" 
            ? "me/drive/root/children" 
            : $"me/drive/root:{parentPath}:/children";

        var payload = new
        {
            name = folderName,
            folder = new { },
            conflictBehavior = "fail"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient!.PostAsync(url, content, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public override async Task<bool> RemoveAsync(string path, CancellationToken cancellationToken = default)
    {
        await EnsureValidTokenAsync(cancellationToken);

        var fullPath = GetFullPath(path);
        var url = $"me/drive/root:{fullPath}";

        var response = await _httpClient!.DeleteAsync(url, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public override async Task<bool> RenameAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        await EnsureValidTokenAsync(cancellationToken);

        var sourceFullPath = GetFullPath(srcPath);
        var destName = Path.GetFileName(dstPath);
        var url = $"me/drive/root:{sourceFullPath}";

        var payload = new { name = destName };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
        {
            Content = content
        };

        var response = await _httpClient!.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public override async Task<bool> MoveAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        await EnsureValidTokenAsync(cancellationToken);

        var sourceFullPath = GetFullPath(srcPath);
        var destFullPath = GetFullPath(dstPath);
        var destParentPath = Path.GetDirectoryName(destFullPath)?.Replace('\\', '/') ?? "/";
        var destName = Path.GetFileName(destFullPath);

        var url = $"me/drive/root:{sourceFullPath}";

        var payload = new
        {
            parentReference = new
            {
                path = string.IsNullOrEmpty(destParentPath) || destParentPath == "/" 
                    ? "/drive/root" 
                    : $"/drive/root:{destParentPath}"
            },
            name = destName
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
        {
            Content = content
        };

        var response = await _httpClient!.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public override async Task<bool> CopyAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        await EnsureValidTokenAsync(cancellationToken);

        var sourceFullPath = GetFullPath(srcPath);
        var destFullPath = GetFullPath(dstPath);
        var destParentPath = Path.GetDirectoryName(destFullPath)?.Replace('\\', '/') ?? "/";
        var destName = Path.GetFileName(destFullPath);

        var url = $"me/drive/root:{sourceFullPath}:/copy";

        var payload = new
        {
            parentReference = new
            {
                path = string.IsNullOrEmpty(destParentPath) || destParentPath == "/" 
                    ? "/drive/root" 
                    : $"/drive/root:{destParentPath}"
            },
            name = destName
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient!.PostAsync(url, content, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private async Task RefreshAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("refresh_token", _refreshToken),
            new KeyValuePair<string, string>("grant_type", "refresh_token")
        });

        var response = await client.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = JsonDocument.Parse(json);
        
        _accessToken = doc.RootElement.GetProperty("access_token").GetString() ?? "";
        if (doc.RootElement.TryGetProperty("refresh_token", out var newRefreshToken))
        {
            _refreshToken = newRefreshToken.GetString() ?? _refreshToken;
        }
        var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60); // 提前60秒刷新

        _httpClient!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    private async Task EnsureValidTokenAsync(CancellationToken cancellationToken = default)
    {
        if (DateTime.UtcNow >= _tokenExpiry)
        {
            await RefreshAccessTokenAsync(cancellationToken);
        }
    }

    private string GetFullPath(string path)
    {
        var cleanPath = NormalizePath(path).TrimStart('/');
        return string.IsNullOrEmpty(_rootPath) 
            ? cleanPath 
            : $"{_rootPath.Trim('/')}/{cleanPath}".TrimStart('/');
    }
}
