using System.Text.Json;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using OpenList.Core.Models;

namespace OpenList.Infrastructure.Drivers;

public class S3StorageDriver : BaseStorageDriver
{
    private AmazonS3Client? _client;
    private string _bucketName = string.Empty;
    private string _prefix = string.Empty;

    public override string Name => "S3";

    public override async Task InitAsync(string configJson, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            var config = JsonDocument.Parse(configJson).RootElement;
            
            var accessKey = config.GetProperty("AccessKey").GetString() 
                ?? throw new InvalidOperationException("AccessKey is required");
            var secretKey = config.GetProperty("SecretKey").GetString() 
                ?? throw new InvalidOperationException("SecretKey is required");
            _bucketName = config.GetProperty("Bucket").GetString() 
                ?? throw new InvalidOperationException("Bucket is required");
            
            var regionStr = config.TryGetProperty("Region", out var region) 
                ? region.GetString() : "us-east-1";
            var endpoint = config.TryGetProperty("Endpoint", out var ep) 
                ? ep.GetString() : null;
            _prefix = config.TryGetProperty("Prefix", out var prefix) 
                ? prefix.GetString() ?? "" : "";

            var s3Config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(regionStr)
            };

            if (!string.IsNullOrEmpty(endpoint))
            {
                s3Config.ServiceURL = endpoint;
                s3Config.ForcePathStyle = true;
            }

            _client = new AmazonS3Client(accessKey, secretKey, s3Config);
            Config = configJson;
        }, cancellationToken);
    }

    public override async Task<FileList> ListAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var key = GetFullKey(path);
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = key.TrimStart('/'),
            Delimiter = "/"
        };

        var response = await _client.ListObjectsV2Async(request, cancellationToken);
        var items = new List<FileItem>();

        // 添加目录
        foreach (var prefix in response.CommonPrefixes)
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
        foreach (var obj in response.S3Objects)
        {
            if (obj.Key.EndsWith('/')) continue; // 跳过目录标记

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
    }

    public override async Task<FileItem?> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var key = GetFullKey(path);
        
        try
        {
            var metadata = await _client.GetObjectMetadataAsync(_bucketName, key.TrimStart('/'), cancellationToken);
            
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
    }

    public override async Task<FileStreamInfo> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var key = GetFullKey(path);
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key.TrimStart('/')
        };

        var response = await _client.GetObjectAsync(request, cancellationToken);
        
        return new FileStreamInfo
        {
            Stream = response.ResponseStream,
            MimeType = response.Headers.ContentType,
            FileName = Path.GetFileName(path)
        };
    }

    public override async Task<bool> UploadAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var key = GetFullKey(path);
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key.TrimStart('/'),
            InputStream = stream
        };

        await _client.PutObjectAsync(request, cancellationToken);
        return true;
    }

    public override async Task<bool> MakeDirAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        // S3 不需要显式创建目录，上传文件时会自动创建
        var key = GetFullKey(path).TrimStart('/').TrimEnd('/') + "/";
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = new MemoryStream()
        };

        await _client.PutObjectAsync(request, cancellationToken);
        return true;
    }

    public override async Task<bool> RemoveAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var key = GetFullKey(path).TrimStart('/');
        
        // 检查是否为目录
        var listRequest = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = key.TrimEnd('/') + "/",
            MaxKeys = 1
        };
        
        var listResponse = await _client.ListObjectsV2Async(listRequest, cancellationToken);
        
        if (listResponse.S3Objects.Count > 0)
        {
            // 删除目录及其所有内容
            var deleteRequest = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = key.TrimEnd('/') + "/"
            };
            
            var deleteResponse = await _client.ListObjectsV2Async(deleteRequest, cancellationToken);
            var deleteObjects = deleteResponse.S3Objects.Select(obj => new KeyVersion { Key = obj.Key }).ToList();
            
            if (deleteObjects.Count > 0)
            {
                var multiDeleteRequest = new DeleteObjectsRequest
                {
                    BucketName = _bucketName,
                    Objects = deleteObjects
                };
                
                await _client.DeleteObjectsAsync(multiDeleteRequest, cancellationToken);
            }
        }
        else
        {
            // 删除单个文件
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };
            
            await _client.DeleteObjectAsync(deleteRequest, cancellationToken);
        }

        return true;
    }

    public override async Task<bool> RenameAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var sourceKey = GetFullKey(srcPath).TrimStart('/');
        var destKey = GetFullKey(dstPath).TrimStart('/');

        // S3 不支持重命名，需要复制后删除
        var copyRequest = new CopyObjectRequest
        {
            SourceBucket = _bucketName,
            SourceKey = sourceKey,
            DestinationBucket = _bucketName,
            DestinationKey = destKey
        };

        await _client.CopyObjectAsync(copyRequest, cancellationToken);

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = sourceKey
        };

        await _client.DeleteObjectAsync(deleteRequest, cancellationToken);

        return true;
    }

    public override async Task<bool> MoveAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        return await RenameAsync(srcPath, dstPath, cancellationToken);
    }

    public override async Task<bool> CopyAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new InvalidOperationException("Driver not initialized");

        var sourceKey = GetFullKey(srcPath).TrimStart('/');
        var destKey = GetFullKey(dstPath).TrimStart('/');

        var copyRequest = new CopyObjectRequest
        {
            SourceBucket = _bucketName,
            SourceKey = sourceKey,
            DestinationBucket = _bucketName,
            DestinationKey = destKey
        };

        await _client.CopyObjectAsync(copyRequest, cancellationToken);

        return true;
    }

    private string GetFullKey(string path)
    {
        var key = NormalizePath(path).TrimStart('/');
        return string.IsNullOrEmpty(_prefix) 
            ? key 
            : $"{_prefix.Trim('/')}/{key}".TrimStart('/');
    }
}
