using System.Text.Json;
using OpenList.Application.DTOs;
using OpenList.Application.Interfaces;
using OpenList.Core.Entities;
using OpenList.Core.Interfaces;

namespace OpenList.Application.Services;

public class StorageService : IStorageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageDriverFactory _driverFactory;

    public StorageService(IUnitOfWork unitOfWork, IStorageDriverFactory driverFactory)
    {
        _unitOfWork = unitOfWork;
        _driverFactory = driverFactory;
    }

    public async Task<StorageDto> CreateStorageAsync(string mountPath, string name, string driver, 
        string configJson, int order, string? remark)
    {
        // 检查挂载路径是否已存在
        var existing = await _unitOfWork.Storages.GetByMountPathAsync(mountPath);
        if (existing != null)
        {
            throw new InvalidOperationException($"挂载路径 '{mountPath}' 已存在");
        }

        // 验证配置 JSON
        try
        {
            JsonDocument.Parse(configJson);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("配置 JSON 格式无效");
        }

        var storage = new Storage
        {
            MountPath = mountPath,
            Name = name,
            Driver = driver,
            ConfigJson = configJson,
            Order = order,
            Remark = remark,
            IsEnabled = true
        };

        await _unitOfWork.Storages.AddAsync(storage);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(storage);
    }

    public async Task<StorageDto?> GetStorageByIdAsync(long id)
    {
        var storage = await _unitOfWork.Storages.GetByIdAsync(id);
        return storage == null ? null : MapToDto(storage);
    }

    public async Task<List<StorageDto>> GetAllStoragesAsync()
    {
        var storages = await _unitOfWork.Storages.GetAllAsync();
        return storages.Select(MapToDto).ToList();
    }

    public async Task<List<StorageDto>> GetEnabledStoragesAsync()
    {
        var storages = await _unitOfWork.Storages.GetEnabledStoragesAsync();
        return storages.Select(MapToDto).ToList();
    }

    public async Task<StorageDto> UpdateStorageAsync(long id, string? name, string? configJson, 
        int? order, string? remark)
    {
        var storage = await _unitOfWork.Storages.GetByIdAsync(id);
        if (storage == null)
        {
            throw new InvalidOperationException("存储配置不存在");
        }

        if (!string.IsNullOrEmpty(name))
        {
            storage.Name = name;
        }

        if (!string.IsNullOrEmpty(configJson))
        {
            // 验证配置 JSON
            try
            {
                JsonDocument.Parse(configJson);
                storage.ConfigJson = configJson;
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("配置 JSON 格式无效");
            }
        }

        if (order.HasValue)
        {
            storage.Order = order.Value;
        }

        if (remark != null)
        {
            storage.Remark = remark;
        }

        _unitOfWork.Storages.Update(storage);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(storage);
    }

    public async Task<bool> DeleteStorageAsync(long id)
    {
        var storage = await _unitOfWork.Storages.GetByIdAsync(id);
        if (storage == null)
        {
            return false;
        }

        _unitOfWork.Storages.Delete(storage);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleStorageAsync(long id)
    {
        var storage = await _unitOfWork.Storages.GetByIdAsync(id);
        if (storage == null)
        {
            throw new InvalidOperationException("存储配置不存在");
        }

        storage.IsEnabled = !storage.IsEnabled;
        _unitOfWork.Storages.Update(storage);
        await _unitOfWork.SaveChangesAsync();

        return storage.IsEnabled;
    }

    public async Task<IStorageDriver> GetDriverAsync(string mountPath, CancellationToken cancellationToken = default)
    {
        var storage = await _unitOfWork.Storages.GetByMountPathAsync(mountPath);
        if (storage == null)
        {
            throw new InvalidOperationException($"挂载路径 '{mountPath}' 不存在");
        }

        if (!storage.IsEnabled)
        {
            throw new InvalidOperationException($"存储 '{storage.Name}' 已被禁用");
        }

        return await _driverFactory.CreateDriverAsync(storage.Driver, storage.ConfigJson, cancellationToken);
    }

    public async Task<bool> TestStorageConnectionAsync(long id, CancellationToken cancellationToken = default)
    {
        var storage = await _unitOfWork.Storages.GetByIdAsync(id);
        if (storage == null)
        {
            throw new InvalidOperationException("存储配置不存在");
        }

        try
        {
            var driver = await _driverFactory.CreateDriverAsync(storage.Driver, storage.ConfigJson, cancellationToken);
            
            // 尝试列出根目录来测试连接
            await driver.ListAsync("/", cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static StorageDto MapToDto(Storage storage)
    {
        return new StorageDto
        {
            Id = (int)storage.Id,
            MountPath = storage.MountPath,
            Name = storage.Name,
            Driver = storage.Driver,
            ConfigJson = storage.ConfigJson,
            Order = storage.Order,
            IsEnabled = storage.IsEnabled,
            Remark = storage.Remark,
            CreatedAt = storage.CreatedAt,
            UpdatedAt = storage.UpdatedAt
        };
    }
}
