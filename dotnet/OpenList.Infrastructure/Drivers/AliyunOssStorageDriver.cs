using System.Text.Json;
using OpenList.Core.Models;

namespace OpenList.Infrastructure.Drivers;

public class AliyunOssStorageDriver : BaseStorageDriver
{
    private Aliyun.OSS.OssClient? _client;
    private string _bucketName = string.Empty;
    private string _prefix = string.Empty;

    public override string Name => "AliyunOSS";

    public override async Task InitAsync(string configJson, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            var config = JsonDocument.Parse(configJson).RootElement;
            
            var endpoint = config.GetProperty("Endpoint").GetString() 
                ?? throw new InvalidOperationException("Endpoint is required");
            var accessKeyId = config.GetProperty("AccessKeyId").GetString() 
                ?? throw new InvalidOperationException("AccessKeyId is required");
            var accessKeySecret = config.GetProperty("AccessKeySecret").GetString() 
                ?? throw new InvalidOperationException("AccessKeySecret is required");
            _bucketName = config.GetProperty("Bucket").GetString() 
                ?? throw new InvalidOperationException("Bucket is required");
            _prefix = config.TryGetProperty("Prefix", out var prefix) 
                ? prefix.GetString() ?? "" : "";

            _client = new Aliyun.OSS.OssClient(endpoint, accessKeyId, accessKeySecret);
            Config = configJson;
        }, cancellationToken);
    }

    public override async Task<FileList> ListAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            var key = GetFullKey(path);
            var request = new Aliyun.OSS.ListObjectsRequest(_bucketName)
            {
                Prefix = key.TrimStart('/'),
                Delimiter = "/",
                MaxKeys = 1000
            };

            var result = _client.ListObjects(request);
            var items = new List<FileItem>();

            // 添加目录
            foreach (var prefix in result.CommonPrefixes)
            {
                var name = prefix.TrimEnd('/').Split('/').Last();
                items.Add(new FileItem
                {
                    Name = name,
                    Path = $"{path.TrimEnd('/')}/{name}",
                    IsDirectory = true,
                    Size = 0,
                    ModifiedTime = DateTime.UtcNow
                });
            }

            // 添加文件
            foreach (var obj in result.ObjectSummaries)
            {
                if (obj.Key.EndsWith('/')) continue;

                var name = obj.Key.Split('/').Last();
                if (string.IsNullOrEmpty(name)) continue;

                items.Add(new FileItem
                {
                    Name = name,
                    Path = $"{path.TrimEnd('/')}/{name}",
                    IsDirectory = false,
                    Size = obj.Size,
                    ModifiedTime = obj.LastModified
                });
            }

            return new FileList
            {
                Items = items,
                Path = path,
                Total = items.Count
            };
        }, cancellationToken);
    }

    public override async Task<FileItem?> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            var key = GetFullKey(path);
            
            try
            {
                var metadata = _client.GetObjectMetadata(_bucketName, key.TrimStart('/'));
                
                return new FileItem
                {
                    Name = Path.GetFileName(path),
                    Path = path,
                    IsDirectory = false,
                    Size = metadata.ContentLength,
                    ModifiedTime = metadata.LastModified
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
            var key = GetFullKey(path);
            var obj = _client.GetObject(_bucketName, key.TrimStart('/'));

            return new FileStreamInfo
            {
                Stream = obj.Content,
                MimeType = obj.Metadata.ContentType,
                FileName = Path.GetFileName(path)
            };
        }, cancellationToken);
    }

    public override async Task<bool> UploadAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            var key = GetFullKey(path);
            _client.PutObject(_bucketName, key.TrimStart('/'), stream);
            return true;
        }, cancellationToken);
    }

    public override async Task<bool> MakeDirAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            // OSS 不需要显式创建目录，创建一个空的目录标记文件
            var key = GetFullKey(path).TrimStart('/').TrimEnd('/') + "/";
            using var emptyStream = new MemoryStream();
            _client.PutObject(_bucketName, key, emptyStream);
            return true;
        }, cancellationToken);
    }

    public override async Task<bool> RemoveAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            var key = GetFullKey(path).TrimStart('/');
            
            // 检查是否为目录
            var listRequest = new Aliyun.OSS.ListObjectsRequest(_bucketName)
            {
                Prefix = key.TrimEnd('/') + "/",
                MaxKeys = 1
            };
            
            var listResult = _client.ListObjects(listRequest);
            
            if (listResult.ObjectSummaries.Count() > 0 || listResult.CommonPrefixes.Count() > 0)
            {
                // 删除目录及其所有内容
                var deleteRequest = new Aliyun.OSS.ListObjectsRequest(_bucketName)
                {
                    Prefix = key.TrimEnd('/') + "/"
                };
                
                var deleteResult = _client.ListObjects(deleteRequest);
                var keys = deleteResult.ObjectSummaries.Select(obj => obj.Key).ToList();
                
                if (keys.Count > 0)
                {
                    var deleteObjectsRequest = new Aliyun.OSS.DeleteObjectsRequest(_bucketName, keys, false);
                    _client.DeleteObjects(deleteObjectsRequest);
                }
            }
            else
            {
                // 删除单个文件
                _client.DeleteObject(_bucketName, key);
            }

            return true;
        }, cancellationToken);
    }

    public override async Task<bool> RenameAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        return await Task.Run(() =>
        {
            var sourceKey = GetFullKey(srcPath).TrimStart('/');
            var destKey = GetFullKey(dstPath).TrimStart('/');

            // OSS 不支持重命名，需要复制后删除
            _client.CopyObject(new Aliyun.OSS.CopyObjectRequest(_bucketName, sourceKey, _bucketName, destKey));
            _client.DeleteObject(_bucketName, sourceKey);

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
            var sourceKey = GetFullKey(srcPath).TrimStart('/');
            var destKey = GetFullKey(dstPath).TrimStart('/');

            _client.CopyObject(new Aliyun.OSS.CopyObjectRequest(_bucketName, sourceKey, _bucketName, destKey));

            return true;
        }, cancellationToken);
    }

    private string GetFullKey(string path)
    {
        var key = NormalizePath(path).TrimStart('/');
        return string.IsNullOrEmpty(_prefix) 
            ? key 
            : $"{_prefix.Trim('/')}/{key}".TrimStart('/');
    }
}
