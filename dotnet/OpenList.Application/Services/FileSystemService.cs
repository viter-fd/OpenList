using OpenList.Application.Interfaces;
using OpenList.Core.Entities;
using OpenList.Core.Interfaces;
using OpenList.Core.Models;

namespace OpenList.Application.Services;

public class FileSystemService : IFileSystemService
{
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public FileSystemService(IStorageService storageService, IUnitOfWork unitOfWork)
    {
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<FileList> ListAsync(string path)
    {
        var (mountPath, relativePath) = ParsePath(path);
        var driver = await _storageService.GetDriverAsync(mountPath);
        return await driver.ListAsync(relativePath);
    }

    public async Task<FileStreamInfo> GetAsync(string path)
    {
        var (mountPath, relativePath) = ParsePath(path);
        var driver = await _storageService.GetDriverAsync(mountPath);
        return await driver.GetFileStreamAsync(relativePath);
    }

    public async Task<bool> UploadAsync(string path, Stream content, string fileName)
    {
        var (mountPath, relativePath) = ParsePath(path);
        var driver = await _storageService.GetDriverAsync(mountPath);
        
        var fullPath = string.IsNullOrEmpty(relativePath) || relativePath == "/" 
            ? $"/{fileName}" 
            : $"{relativePath.TrimEnd('/')}/{fileName}";
        
        return await driver.UploadAsync(fullPath, content);
    }

    public async Task<bool> MakeDirAsync(string path)
    {
        var (mountPath, relativePath) = ParsePath(path);
        var driver = await _storageService.GetDriverAsync(mountPath);
        return await driver.MakeDirAsync(relativePath);
    }

    public async Task<bool> MoveAsync(string sourcePath, string destPath)
    {
        var (sourceMountPath, sourceRelativePath) = ParsePath(sourcePath);
        var (destMountPath, destRelativePath) = ParsePath(destPath);

        // 检查是否在同一存储
        if (sourceMountPath != destMountPath)
        {
            throw new InvalidOperationException("不支持跨存储移动文件");
        }

        var driver = await _storageService.GetDriverAsync(sourceMountPath);
        return await driver.MoveAsync(sourceRelativePath, destRelativePath);
    }

    public async Task<bool> CopyAsync(string sourcePath, string destPath)
    {
        var (sourceMountPath, sourceRelativePath) = ParsePath(sourcePath);
        var (destMountPath, destRelativePath) = ParsePath(destPath);

        // 检查是否在同一存储
        if (sourceMountPath != destMountPath)
        {
            // 跨存储复制需要下载再上传
            var sourceDriver = await _storageService.GetDriverAsync(sourceMountPath);
            var destDriver = await _storageService.GetDriverAsync(destMountPath);

            var fileStreamInfo = await sourceDriver.GetFileStreamAsync(sourceRelativePath);
            return await destDriver.UploadAsync(destRelativePath, fileStreamInfo.Stream);
        }

        var driver = await _storageService.GetDriverAsync(sourceMountPath);
        return await driver.CopyAsync(sourceRelativePath, destRelativePath);
    }

    public async Task<bool> RemoveAsync(string path)
    {
        var (mountPath, relativePath) = ParsePath(path);
        var driver = await _storageService.GetDriverAsync(mountPath);
        return await driver.RemoveAsync(relativePath);
    }

    public async Task<bool> RenameAsync(string path, string newName)
    {
        var (mountPath, relativePath) = ParsePath(path);
        var driver = await _storageService.GetDriverAsync(mountPath);
        return await driver.RenameAsync(relativePath, newName);
    }

    public async Task<List<FileItem>> SearchAsync(string keyword, string? mountPath = null)
    {
        var results = new List<FileItem>();

        // 获取要搜索的存储列表
        var storages = string.IsNullOrEmpty(mountPath)
            ? await _storageService.GetEnabledStoragesAsync()
            : new List<DTOs.StorageDto> { await _storageService.GetStorageByIdAsync(int.Parse(mountPath)) 
                ?? throw new InvalidOperationException("存储不存在") };

        foreach (var storage in storages)
        {
            try
            {
                var driver = await _storageService.GetDriverAsync(storage.MountPath);
                var files = await SearchInStorageAsync(driver, "/", keyword);
                
                // 添加挂载路径前缀
                foreach (var file in files)
                {
                    file.Path = $"{storage.MountPath}{file.Path}";
                }
                
                results.AddRange(files);
            }
            catch
            {
                // 忽略单个存储的搜索错误
            }
        }

        return results;
    }

    private async Task<List<FileItem>> SearchInStorageAsync(IStorageDriver driver, string path, string keyword)
    {
        var results = new List<FileItem>();
        
        try
        {
            var fileList = await driver.ListAsync(path);
            
            foreach (var item in fileList.Items)
            {
                // 检查文件名是否包含关键字
                if (item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(item);
                }

                // 如果是目录，递归搜索
                if (item.IsDirectory)
                {
                    var subResults = await SearchInStorageAsync(driver, item.Path, keyword);
                    results.AddRange(subResults);
                }
            }
        }
        catch
        {
            // 忽略无法访问的目录
        }

        return results;
    }

    private (string mountPath, string relativePath) ParsePath(string path)
    {
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            throw new InvalidOperationException("路径必须包含挂载点");
        }

        var parts = path.TrimStart('/').Split('/', 2);
        var mountPath = $"/{parts[0]}";
        var relativePath = parts.Length > 1 ? $"/{parts[1]}" : "/";

        return (mountPath, relativePath);
    }
}
