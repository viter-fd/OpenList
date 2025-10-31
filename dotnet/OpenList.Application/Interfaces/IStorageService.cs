using OpenList.Application.DTOs;
using OpenList.Core.Interfaces;

namespace OpenList.Application.Interfaces;

/// <summary>
/// 存储服务接口
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// 创建存储
    /// </summary>
    Task<StorageDto> CreateStorageAsync(string mountPath, string name, string driver, string configJson, int order, string? remark);

    /// <summary>
    /// 根据ID获取存储
    /// </summary>
    Task<StorageDto?> GetStorageByIdAsync(long id);

    /// <summary>
    /// 获取所有存储
    /// </summary>
    Task<List<StorageDto>> GetAllStoragesAsync();

    /// <summary>
    /// 获取已启用的存储
    /// </summary>
    Task<List<StorageDto>> GetEnabledStoragesAsync();

    /// <summary>
    /// 更新存储
    /// </summary>
    Task<StorageDto> UpdateStorageAsync(long id, string? name, string? configJson, int? order, string? remark);

    /// <summary>
    /// 删除存储
    /// </summary>
    Task<bool> DeleteStorageAsync(long id);

    /// <summary>
    /// 启用/禁用存储
    /// </summary>
    Task<bool> ToggleStorageAsync(long id);

    /// <summary>
    /// 获取存储驱动
    /// </summary>
    Task<IStorageDriver> GetDriverAsync(string mountPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 测试存储连接
    /// </summary>
    Task<bool> TestStorageConnectionAsync(long id, CancellationToken cancellationToken = default);
}
